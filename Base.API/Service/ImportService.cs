using Base.IService.IService;
using Base.Service.IService;
using CloudinaryDotNet.Actions;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using Google.Type;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;


namespace Base.API.Service;

public class ImportService
{
    private readonly ISemesterService _semesterService;
    private readonly ISlotService _slotService;
    private readonly IClassService _classService;

    public ImportService(ISemesterService semesterService, ISlotService slotService, IClassService classService)
    {
        _semesterService = semesterService;
        _slotService = slotService;
        _classService = classService;
    }

    public async Task<Import_Result> ImportScheduleUsingImage(IFormFile imageResource, int semesterId, Guid lecturerId, int? recommendationRate)
    {
        var credential = GoogleCredential.FromFile("keys/next-project-426205-5bd6e4b638be.json");
        ImageAnnotatorClientBuilder imageAnnotatorClientBuilder = new ImageAnnotatorClientBuilder();
        imageAnnotatorClientBuilder.Credential = credential;
        var client = imageAnnotatorClientBuilder.Build();
        Image image = Image.FromStream(imageResource.OpenReadStream());

        // Perform text detection on the image
        var response = await client.DetectDocumentTextAsync(image);

        //===============================Other datas to work with===============================
        // year
        bool isYear = false;
        int? year = null;

        // Date
        var weeklyDates = new List<Import_Date>();
        bool loadDate = true;
        bool isDate = false;

        // Slot
        var slots = new List<Import_Slot>();
        //===============================Other datas to work with===============================

        foreach (var page in response.Pages)
        {
            foreach (var block in page.Blocks)
            {
                string blockText = "";
                string text = "";
                foreach (var paragraph in block.Paragraphs)
                {
                    foreach (var word in paragraph.Words)
                    {
                        string wordText = "";
                        foreach (var symbol in word.Symbols)
                        {
                            blockText += symbol.Text;
                            wordText += symbol.Text;
                        }

                        // Identify whether if it is year
                        if (isYear)
                        {
                            year = IdentifyYear(wordText);
                            isYear = false;
                        }
                        if (wordText == "YEAR")
                        {
                            isYear = true;
                        }

                        //blockText += " "; // Add space after each word
                    }
                }


                // =================================HANDLE BLOCK TEXT HERE=================================
                blockText = blockText.Trim();
                text = blockText;


                // =================================Handle date only data in weekly timetable=================================
                // The data for date of weekly timetable seems only appear on 15 first string
                if (loadDate)
                {
                    DateOnly date;
                    var checkDateOnly = DateOnly.TryParseExact(blockText, "dd/MM", out date);
                    if (checkDateOnly)
                    {
                        weeklyDates.Add(new Import_Date
                        {
                            DateString = blockText,
                            Date = date,
                            Vertex_X = block.BoundingBox.Vertices[0].X
                        });
                        isDate = true;
                        continue;
                    }
                    else
                    {
                        if (isDate)
                        {
                            isDate = false;
                            loadDate = false;
                        }
                    }
                }


                // =================================Handle slot data in weekly timetable=================================
                int slotNumber = 0;
                if (IdentifySlot(blockText, out slotNumber))
                {
                    slots.Add(new Import_Slot
                    {
                        SlotNumber = slotNumber,
                        Vertex_Y = block.BoundingBox.Vertices[0].Y
                    });
                    continue;
                }


                // =================================Handle attendance word=================================
                blockText = DeleteAttendedWord(blockText);


                // =================================Handle class slot here=================================
                if (blockText is not null && blockText != string.Empty && blockText != "")
                {
                    var vertex_y = block.BoundingBox.Vertices[0].Y;
                    var slot = slots.Where(s => s.CheckVertex_Y(vertex_y, 10)).FirstOrDefault();
                    if (slot is not null)
                    {
                        slot.ClassSlots.Add(new Import_Class
                        {
                            ClassCode = blockText,
                            Vertex_X = block.BoundingBox.Vertices[0].X
                        });
                    }
                }
            }
        }

        // All data is ready to validate from here
        // Validate slot
        foreach(var slot in slots)
        {
            var getSlotResult = await _slotService.GetAllSlots(1, 1, 10, slot.SlotNumber, null, null);
            if (getSlotResult.IsSuccess)
            {
                var existedSlot = getSlotResult.Result?.FirstOrDefault();
                if(existedSlot is not null)
                {
                    slot.SlotOrder = existedSlot.Order;
                }
                else
                {
                    slots.Remove(slot);
                }
            }
            else
            {
                slots.Remove(slot);
            }
        }

        // Validate schedule of each slot
        var existedClassCode = await _classService.GetAllClassCodes(semesterId, lecturerId);
        var validateSchedulesTasks = new List<Task<Import_Slot>>();
        foreach(var slot in slots)
        {
            validateSchedulesTasks.Add(ValidateSchedule(slot, existedClassCode.Where(c => c != string.Empty).ToList(), recommendationRate));
        }
        var validateSchedulesResult = await Task.WhenAll(validateSchedulesTasks);
        if(validateSchedulesResult is not null && validateSchedulesResult.Length > 0)
        {
            slots = validateSchedulesResult.ToList();
        }

        // Sort
        weeklyDates = weeklyDates.OrderBy(s => s.Date).ToList();
        slots = slots.OrderBy(s => s.SlotOrder).ToList();


        // Adjust class slot data of each slot
        var weeklyDatesPosition = weeklyDates.Select(d => d.Vertex_X);
        int totalDates = weeklyDates.Count();
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.3 * 2))
        };
        Parallel.ForEach(slots, parallelOptions, (slot, state) =>
        {
            var adjustedClassSlots = new Import_Class?[totalDates];
            for(int i = 0; i < totalDates - 1; i++)
            {
                var acceptedClassSlot = slot.ClassSlots.Where(c => c != null && c.CheckVertex_X(weeklyDatesPosition.ElementAt(i), 10)).FirstOrDefault();
                adjustedClassSlots[i] = acceptedClassSlot;
            }
            slot.AdjustedClassSlots = adjustedClassSlots.ToList();
            slot.ClassSlots = Enumerable.Empty<Import_Class>().ToList();
        });

        return new Import_Result
        {
            Year = year ?? 0,
            DatesCount = weeklyDates.Count,
            SlotsCount = slots.Count,
            Dates = weeklyDates,
            Slots = slots
        };
    }

    public async Task<Import_Result> ImportScheduleUsingImageV2(IFormFile imageResource, int semesterId, Guid lecturerId, int? recommendationRate)
    {
        var credential = GoogleCredential.FromFile("keys/next-project-426205-5bd6e4b638be.json");
        ImageAnnotatorClientBuilder imageAnnotatorClientBuilder = new ImageAnnotatorClientBuilder();
        imageAnnotatorClientBuilder.Credential = credential;
        var client = imageAnnotatorClientBuilder.Build();
        Image image = Image.FromStream(imageResource.OpenReadStream());

        // Perform text detection on the image
        var response = await client.DetectDocumentTextAsync(image);

        //===============================Other datas to work with===============================
        // year
        bool isYear = false;
        int? year = null;

        // Date
        var weeklyDates = new List<Import_Date>();
        bool loadDate = true;
        bool isDate = false;

        // Slot
        var slots = new List<Import_Slot>();
        //===============================Other datas to work with===============================



        // Ordered textblock
        var textBlocks = new List<TextBlock>();
        foreach (var page in response.Pages)
        {
            foreach (var block in page.Blocks)
            {
                string blockText = "";
                var paragraphs = new List<Paragraph>();
                foreach (var paragraph in block.Paragraphs)
                {
                    string paragraphText = "";
                    var words = new List<Word>();
                    foreach (var word in paragraph.Words)
                    {
                        string wordText = "";
                        foreach (var symbol in word.Symbols)
                        {
                            blockText += symbol.Text;
                            paragraphText += symbol.Text;
                            wordText += symbol.Text;
                        }
                        //blockText += " "; // Add space after each word
                        words.Add(new Word { Text = wordText });
                    }
                    paragraphs.Add(new Paragraph
                    {
                        Text = paragraphText,
                        Words = words
                    });
                }

                textBlocks.Add(new TextBlock
                {
                    Text = blockText.Trim(),
                    StartGeometricCoordinates = new GeometricCoordinates(block.BoundingBox.Vertices[0].X, block.BoundingBox.Vertices[0].Y),
                    EndGeometricCoordinates = new GeometricCoordinates(block.BoundingBox.Vertices[1].X, block.BoundingBox.Vertices[1].Y),
                    Paragraphs = paragraphs
                });
            }
        }
        var sortedTextBlocks = SortTextBlocks(textBlocks);
        var adjustedTextBlocks = sortedTextBlocks
            .Where(b => (b.Text.ToUpper() != "(ATTENDED)") &&
                  (!b.Text.ToUpper().Contains("(NOTYET)")))
            .ToList();


        // Identify year and date should base on words, not paragraph
        // Get TextBlock of year
        var yearTextBlock = adjustedTextBlocks.Where(b => b.Paragraphs.Any(p => p.Words.Any(w => w.Text.ToUpper() == "YEAR"))).FirstOrDefault();
        int getYear = 0;
        var tryGetYear = int.TryParse(yearTextBlock?.Paragraphs.SelectMany(p => p.Words).Where(w => w.Text.ToUpper() != "YEAR").FirstOrDefault()?.Text, out getYear);
        if (yearTextBlock is not null)
            adjustedTextBlocks.Remove(yearTextBlock);


        // Get TextBlock of date
        DateOnly testDate;
        var dateBlock = new List<TextBlock>();
        int dateBlockIndex = 0;
        var dateBlockIndexs = new List<int>();
        foreach (var block in adjustedTextBlocks)
        {
            if (block.Paragraphs.Any(p => p.Words.Any(w => DateOnly.TryParseExact(w.Text, "dd/MM", out testDate))))
            {
                dateBlock.Add(block);
                dateBlockIndexs.Add(dateBlockIndex);
            }
            ++dateBlockIndex;
        }
        dateBlockIndexs.Reverse();
        foreach (int index in dateBlockIndexs)
        {
            adjustedTextBlocks.RemoveAt(index);
        }


        // Get TextBlock of slot
        var slotBlock = new List<TextBlock>();
        int slotBlockIndex = 0;
        var slotBlockIndexs = new List<int>();
        foreach (var block in adjustedTextBlocks)
        {
            if (block.Paragraphs.Any(p => p.Words.Any(w => w.Text.ToUpper() == "SLOT")))
            {
                slotBlock.Add(block);
                slotBlockIndexs.Add(slotBlockIndex);
            }
            ++slotBlockIndex;
        }
        slotBlockIndexs.Reverse();
        foreach (var index in slotBlockIndexs)
        {
            adjustedTextBlocks.RemoveAt(index);
        }


        // Sort 


        foreach (var page in response.Pages)
        {
            foreach (var block in page.Blocks)
            {
                string blockText = "";
                string text = "";
                foreach (var paragraph in block.Paragraphs)
                {
                    foreach (var word in paragraph.Words)
                    {
                        string wordText = "";
                        foreach (var symbol in word.Symbols)
                        {
                            blockText += symbol.Text;
                            wordText += symbol.Text;
                        }

                        // Identify whether if it is year
                        if (isYear)
                        {
                            year = IdentifyYear(wordText);
                            isYear = false;
                        }
                        if (wordText == "YEAR")
                        {
                            isYear = true;
                        }

                        //blockText += " "; // Add space after each word
                    }
                }


                // =================================HANDLE BLOCK TEXT HERE=================================
                blockText = blockText.Trim();
                text = blockText;


                // =================================Handle date only data in weekly timetable=================================
                // The data for date of weekly timetable seems only appear on 15 first string
                if (loadDate)
                {
                    DateOnly date;
                    var checkDateOnly = DateOnly.TryParseExact(blockText, "dd/MM", out date);
                    if (checkDateOnly)
                    {
                        weeklyDates.Add(new Import_Date
                        {
                            DateString = blockText,
                            Date = date,
                            Vertex_X = block.BoundingBox.Vertices[0].X
                        });
                        isDate = true;
                        continue;
                    }
                    else
                    {
                        if (isDate)
                        {
                            isDate = false;
                            loadDate = false;
                        }
                    }
                }


                // =================================Handle slot data in weekly timetable=================================
                int slotNumber = 0;
                if (IdentifySlot(blockText, out slotNumber))
                {
                    slots.Add(new Import_Slot
                    {
                        SlotNumber = slotNumber,
                        Vertex_Y = block.BoundingBox.Vertices[0].Y
                    });
                    continue;
                }


                // =================================Handle attendance word=================================
                blockText = DeleteAttendedWord(blockText);


                // =================================Handle class slot here=================================
                if (blockText is not null && blockText != string.Empty && blockText != "")
                {
                    var vertex_y = block.BoundingBox.Vertices[0].Y;
                    var slot = slots.Where(s => s.CheckVertex_Y(vertex_y, 10)).FirstOrDefault();
                    if (slot is not null)
                    {
                        slot.ClassSlots.Add(new Import_Class
                        {
                            ClassCode = blockText,
                            Vertex_X = block.BoundingBox.Vertices[0].X
                        });
                    }
                }
            }
        }

        // All data is ready to validate from here
        // Validate slot
        foreach (var slot in slots)
        {
            var getSlotResult = await _slotService.GetAllSlots(1, 1, 10, slot.SlotNumber, null, null);
            if (getSlotResult.IsSuccess)
            {
                var existedSlot = getSlotResult.Result?.FirstOrDefault();
                if (existedSlot is not null)
                {
                    slot.SlotOrder = existedSlot.Order;
                }
                else
                {
                    slots.Remove(slot);
                }
            }
            else
            {
                slots.Remove(slot);
            }
        }

        // Validate schedule of each slot
        var existedClassCode = await _classService.GetAllClassCodes(semesterId, lecturerId);
        var validateSchedulesTasks = new List<Task<Import_Slot>>();
        foreach (var slot in slots)
        {
            validateSchedulesTasks.Add(ValidateSchedule(slot, existedClassCode.Where(c => c != string.Empty).ToList(), recommendationRate));
        }
        var validateSchedulesResult = await Task.WhenAll(validateSchedulesTasks);
        if (validateSchedulesResult is not null && validateSchedulesResult.Length > 0)
        {
            slots = validateSchedulesResult.ToList();
        }

        // Sort
        weeklyDates = weeklyDates.OrderBy(s => s.Date).ToList();
        slots = slots.OrderBy(s => s.SlotOrder).ToList();


        // Adjust class slot data of each slot
        var weeklyDatesPosition = weeklyDates.Select(d => d.Vertex_X);
        int totalDates = weeklyDates.Count();
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.3 * 2))
        };
        Parallel.ForEach(slots, parallelOptions, (slot, state) =>
        {
            var adjustedClassSlots = new Import_Class?[totalDates];
            for (int i = 0; i < totalDates - 1; i++)
            {
                var acceptedClassSlot = slot.ClassSlots.Where(c => c != null && c.CheckVertex_X(weeklyDatesPosition.ElementAt(i), 10)).FirstOrDefault();
                adjustedClassSlots[i] = acceptedClassSlot;
            }
            slot.AdjustedClassSlots = adjustedClassSlots.ToList();
            slot.ClassSlots = Enumerable.Empty<Import_Class>().ToList();
        });

        return new Import_Result
        {
            Year = year ?? 0,
            DatesCount = weeklyDates.Count,
            SlotsCount = slots.Count,
            Dates = weeklyDates,
            Slots = slots
        };
    }

    private string DeleteAttendedWord(string text)
    {
        string searchString = "(ATTENDED)";
        var upperText = text.ToUpper();
        if (upperText.Contains("(ATTENDED)"))
        {
            int startIndex = upperText.IndexOf(searchString);
            text = text.Remove(startIndex, searchString.Length);
        }
        return text;
    }

    private int? IdentifyYear(string yearString)
    {
        int year = 0;
        bool yearParseResult = int.TryParse(yearString, out year);
        if (yearParseResult)
        {
            if (year < 2000 || year > 2050)
            {
                return null;
            }
        }
        else
        {
            return null;
        }
        return year;
    }

    private bool IdentifySlot(string text, out int slotNumber)
    {
        string searchString = "SLOT";
        var upperText = text.ToUpper();
        if (upperText.Contains(searchString))
        {
            int startIndex = upperText.IndexOf(searchString);
            text = text.Remove(startIndex, searchString.Length);
            if (int.TryParse(text, out slotNumber))
            {
                return true;
            }
        }

        slotNumber = 0;
        return false;
    }

    private Task<Import_Slot> ValidateSchedule(Import_Slot slot, List<string> existedClassCodes, int? recommedationRate)
    {
        return Task.Run(() =>
        {
            ConcurrentBag<Import_Class> classes = new ConcurrentBag<Import_Class>();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.3 * 2))
            };

            Parallel.ForEach(slot.ClassSlots, parallelOptions, (classSlot, state) =>
            {
                if (existedClassCodes.Any(c => c == classSlot.ClassCode.ToUpper()))
                {
                    classes.Add(classSlot);
                }
                else
                {
                    var importClass = CheckClassCode(classSlot, ref existedClassCodes, recommedationRate);
                    if (importClass is not null)
                    {
                        classes.Add(importClass);
                    }
                }
            });

            slot.ClassSlots = classes.ToList();

            return slot;
        });
    }

    private Import_Class? CheckClassCode(Import_Class classSlot, ref List<string> existedClassCodes, int? recommendationRate)
    {
        var recommendations = new List<Class_Suggest>();

        var classCode = classSlot.ClassCode.ToUpper();
        foreach (var item in existedClassCodes)
        {
            if (item.Length > classCode.Length) continue;

            int correctCount = 0;
            for(int i = 0; i < item.Length - 1; i++)
            {
                if (item[i] == classCode[i])
                {
                    correctCount++;
                }
            }
            int correctRate = (correctCount * 100 / item.Length);
            if (correctRate >= (recommendationRate ?? 50))
            {
                recommendations.Add(new Class_Suggest
                {
                    ClassCode = item,
                    SuggestRate = correctRate,
                    CorrectCount = correctCount
                });
            }
        }

        if(recommendations.Count() == 0)
        {
            return null;
        }

        classSlot.Recommendations = recommendations.OrderByDescending(r => r.SuggestRate);
        classSlot.ClassCode = classSlot.Recommendations.First().ClassCode;

        return classSlot;
    }




    private List<TextBlock> SortTextBlocks(List<TextBlock> textBlocks)
    {
        return textBlocks.OrderBy(b => b.StartGeometricCoordinates.Vertex_Y)
                         .ThenBy(b => b.StartGeometricCoordinates.Vertex_X)
                         .ToList();
    }
}

public class Import_Slot
{
    public int SlotNumber { get; set; } = 0;
    public int SlotOrder { get; set; } = 0;
    public int Vertex_Y { get; set; } = 0;
    public List<Import_Class> ClassSlots { get; set; } = new List<Import_Class>();
    public List<Import_Class?> AdjustedClassSlots { get; set; } = new List<Import_Class?>();

    public bool CheckVertex_Y(int vertex_y, int acceptError)
    {
        return ((Vertex_Y - acceptError) <= vertex_y && vertex_y <= (Vertex_Y + acceptError));
    }
}

public class Import_Date
{
    public string DateString { get; set; } = "";
    public DateOnly Date { get; set; }
    public int Vertex_X { get; set; } = 0;
}

public class Import_Class
{
    public string ClassCode { get; set; } = "";
    public int Vertex_X { get; set; } = 0;
    public IEnumerable<Class_Suggest> Recommendations { get; set; } = new List<Class_Suggest>();

    public bool CheckVertex_X(int vertex_x, int acceptError)
    {
        return ((Vertex_X - acceptError) <= vertex_x && vertex_x <= (Vertex_X + acceptError));
    }
}

public class Class_Suggest
{
    public string ClassCode { get; set; } = "Empty";
    public int SuggestRate { get; set; } = 0;
    public int CorrectCount { get; set; } = 0;
}

public class Import_Result
{
    public int Year { get; set; }
    public int DatesCount { get; set; }
    public int SlotsCount { get; set; }
    public IEnumerable<Import_Date> Dates { get; set; } = new List<Import_Date>();
    public IEnumerable<Import_Slot> Slots { get; set; } = new List<Import_Slot>();
}


public class TextBlock
{
    public string Text { get; set; } = "";
    public GeometricCoordinates StartGeometricCoordinates { get; set; } = new GeometricCoordinates(0, 0);
    public GeometricCoordinates EndGeometricCoordinates { get; set; } = new GeometricCoordinates(0, 0);
    public IEnumerable<Paragraph> Paragraphs { get; set; } = new List<Paragraph>();
}

public class GeometricCoordinates
{
    public GeometricCoordinates(int vertexX, int vertexY)
    {
        Vertex_X = vertexX;
        Vertex_Y = vertexY;
    }
    public int Vertex_X { get; set; }
    public int Vertex_Y { get; set; }
}

public class Paragraph
{
    public string Text { get; set; } = "";
    public IEnumerable<Word> Words { get; set; } = new List<Word>();
}

public class Word
{ 
    public string Text { get; set; } = "";
}