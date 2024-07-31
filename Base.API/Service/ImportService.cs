using Base.IService.IService;
using Base.Service.IService;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
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
}

public class Import_Slot
{
    public int SlotNumber { get; set; } = 0;
    public int SlotOrder { get; set; } = 0;
    public int Vertex_Y { get; set; } = 0;
    public List<Import_Class> ClassSlots { get; set; } = new List<Import_Class>();

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
