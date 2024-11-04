using Base.IService.IService;
using Base.Repository.Entity;
using Base.Service.IService;
using CloudinaryDotNet.Actions;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using Google.Type;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;


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

    public async Task<Import_Result> ImportScheduleUsingImageV2(IFormFile imageResource, int semesterId, Guid lecturerId, int? recommendationRate)
    {
        var credential = GoogleCredential.FromFile("keys/sams-capstone-project-f08e0cb36d56.json");
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
            var getSlotResult = await _slotService.GetAllSlots(1, 1, 10, slot.SlotNumber, null, null, null);
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

    public async Task<Import_Result> ImportScheduleUsingImage(IFormFile imageResource, Guid lecturerId, int recommendationRate = 50)
    {
        var credential = GoogleCredential.FromFile("keys/sams-capstone-project-4d911153636d.json");
        ImageAnnotatorClientBuilder imageAnnotatorClientBuilder = new ImageAnnotatorClientBuilder();
        imageAnnotatorClientBuilder.Credential = credential;
        var client = imageAnnotatorClientBuilder.Build();
        Image image = Image.FromStream(imageResource.OpenReadStream());

        // Perform text detection on the image
        var response = await client.DetectDocumentTextAsync(image);

        //===============================Other datas to work with===============================
        // year
        int? year = null;

        // Date
        var weeklyDates = new List<Import_Date>();

        // Slot
        var slots = new List<Import_Slot>();

        Semester? existedSemester;
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
                    ModifyGeometricCoordinates = new GeometricCoordinates(block.BoundingBox.Vertices[2].X, block.BoundingBox.Vertices[2].Y),
                    Paragraphs = paragraphs
                });
            }
        }
        var sortedTextBlocks = SortTextBlocks(textBlocks);
        var parallelOption = new ParallelOptions
        {
            MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.3 * 2))
        };

        // Remove unnecessary words
        var modifiedTextBlocks = new ConcurrentBag<TextBlock>();
        Parallel.ForEach(sortedTextBlocks, parallelOption, (textBlock, state) =>
        {
            var upperText = textBlock.Text.ToUpper();
            // Remove word edunext, notyet and attended
            if (upperText.Contains("NOTYET") || upperText.Contains("ATTENDED") || upperText.Contains("EDUNEXT"))
            {
                var newParagraphs = new List<Paragraph>();
                foreach (var paragraph in textBlock.Paragraphs)
                {
                    // Only get valid paragraph in the text block
                    paragraph.Words = paragraph.Words
                        .Where(p => p.Text != "(" && 
                            p.Text != ")" &&
                            !p.Text.ToUpper().Contains("NOTYET") && 
                            !p.Text.ToUpper().Contains("ATTENDED") && 
                            !p.Text.ToUpper().Contains("EDUNEXT"));
                    if(paragraph.Words.Count() > 0)
                    {
                        paragraph.Text = string.Join("", paragraph.Words.Select(w => w.Text));
                        newParagraphs.Add(paragraph);
                    }
                }

                if (newParagraphs.Count() > 0)
                {
                    textBlock.Paragraphs = newParagraphs;
                    textBlock.Text = string.Concat(textBlock.Paragraphs.Select(p => p.Text));
                    modifiedTextBlocks.Add(textBlock);
                }

/*                var modifiedParagraph = textBlock.Paragraphs
                        .Where(p => !p.Text.ToUpper().Contains("(NOTYET)") &&
                            !p.Text.ToUpper().Contains("(ATTENDED)"));
                if (modifiedParagraph.Count() > 0)
                {
                    textBlock.Paragraphs = modifiedParagraph;
                    textBlock.Text = string.Concat(textBlock.Paragraphs.Select(p => p.Text));
                    modifiedTextBlocks.Add(textBlock);
                }
                else
                {

                }*/
            }
            else
            {
                modifiedTextBlocks.Add(textBlock);
            }
        });
        var adjustedTextBlocks = modifiedTextBlocks.ToList();

        /*var adjustedTextBlocks = sortedTextBlocks
            .Where(b => (b.Text.ToUpper() != "(ATTENDED)") &&
                  (!b.Text.ToUpper().Contains("(NOTYET)")))
            .ToList();*/


        // Identify year and date should base on words, not paragraph
        // Get TextBlock of year
        var yearTextBlock = adjustedTextBlocks.Where(b => b.Paragraphs.Any(p => p.Words.Any(w => w.Text.ToUpper() == "YEAR"))).FirstOrDefault();
        int getYear = 0;
        var tryGetYear = int.TryParse(yearTextBlock?.Paragraphs.SelectMany(p => p.Words).Where(w => w.Text.ToUpper() != "YEAR").FirstOrDefault()?.Text, out getYear);
        if (tryGetYear)
        {
            year = getYear;
        }
        if (yearTextBlock is not null)
            adjustedTextBlocks.Remove(yearTextBlock);


        // Get TextBlock of date
        DateOnly testDate;
        var dateBlock = new List<TextBlock>();
        int dateBlockIndex = 0;
        var dateBlockIndexs = new HashSet<int>();
        foreach (var block in adjustedTextBlocks)
        {
            if (block.Paragraphs.Any(p => !p.Words.Any(w => w.Text.ToUpper() == "SLOT") && p.Words.Any(w => DateOnly.TryParseExact(w.Text, "dd/MM", out testDate))))
            {
                dateBlock.Add(block);
                dateBlockIndexs.Add(dateBlockIndex);
            }
            ++dateBlockIndex;
        }
        var usingDateBlockIndexs = dateBlockIndexs.OrderByDescending(x => x);
        foreach (int index in usingDateBlockIndexs)
        {
            adjustedTextBlocks.RemoveAt(index);
        }
        dateBlock = dateBlock.Where(b => b.Paragraphs.Any(p => !p.Words.Any(w => w.Text.ToUpper() == "TO" || w.Text.ToUpper() == "WEEK"))).ToList();
        foreach(var block in dateBlock)
        {
            DateOnly date;
            foreach (var paragraph in block.Paragraphs)
            {
                var numberOfWord = paragraph.Words.Count();
                var startX = block.StartGeometricCoordinates.Vertex_X;
                var endX = block.EndGeometricCoordinates.Vertex_X;
                int count = 0;
                foreach(var word in paragraph.Words)
                {
                    if (DateOnly.TryParseExact(word.Text, "dd/MM", out date))
                    {
                        // Should identify the vertext X if there are 2 days are combined
                        float rate = (float)count / numberOfWord;
                        var finalX = startX + (int)((endX - startX) * rate);
                        weeklyDates.Add(new Import_Date
                        {
                            DateString = word.Text,
                            Date = date,
                            Vertex_X = finalX
                        });
                    }
                    count++;
                }
            }
        }


        // Get TextBlock of slot
        var slotBlock = new List<TextBlock>();
        int slotBlockIndex = 0;
        var slotBlockIndexs = new HashSet<int>();
        foreach (var block in adjustedTextBlocks)
        {
            // Handle the case that a block have multiple paragraphs of slot
            if (block.Paragraphs.Any(p => p.Words.Any(w => w.Text.ToUpper().Contains("SLOT"))))
            {
                var words = block.Paragraphs.SelectMany(p => p.Words);
                var slotWordsCount = words.Where(w => w.Text.ToUpper().Contains("SLOT")).Count();
                if (slotWordsCount > 1)
                {
                    int slotWordNumber = 0;
                    for(int i = 0; i < words.Count() - 1; i++)
                    {
                        var word = words.ElementAtOrDefault(i);
                        if (word is not null && word.Text.ToUpper().Contains("SLOT"))
                        {
                            List<Word> newWords = new List<Word>();
                            newWords.Add(word);
                            var newParagraphs = new List<Paragraph>();
                            if(word.Text.ToUpper() == "SLOT")
                            {
                                var slotNumberWord = words.ElementAtOrDefault(i + 1);
                                if(slotNumberWord is not null)
                                {
                                    newWords.Add(slotNumberWord);
                                }
                            }

                            newParagraphs.Add(new Paragraph
                            {
                                Text = string.Join("", newWords.Select(w => w.Text)),
                                Words = newWords
                            });

                            float rate = (float)slotWordNumber / slotWordsCount;
                            var newVertexY = block.StartGeometricCoordinates.Vertex_Y + (int)Math.Round((block.ModifyGeometricCoordinates.Vertex_Y - block.StartGeometricCoordinates.Vertex_Y) * rate, 0);
                            var newGeometricCoordinates = new GeometricCoordinates(block.StartGeometricCoordinates.Vertex_X, newVertexY);

                            slotBlock.Add(new TextBlock
                            {
                                Text = string.Join("", newParagraphs.Select(p => p.Text)),
                                Paragraphs = newParagraphs,
                                StartGeometricCoordinates = newGeometricCoordinates
                            });
                            slotBlockIndexs.Add(slotBlockIndex);

                            ++slotWordNumber;
                        }
                    }
                }
                else if(slotWordsCount == 1)
                {
                    float rate = 0;
                    var indexOfSlotWord = (float)words.TakeWhile(w => !w.Text.ToUpper().Contains("SLOT")).ToList().Count() + 1;
                    if(words.Count() > 3)
                    {
                        rate = indexOfSlotWord / words.Count();
                    }
                    var newVertexY = block.StartGeometricCoordinates.Vertex_Y + (int)Math.Round((block.ModifyGeometricCoordinates.Vertex_Y - block.StartGeometricCoordinates.Vertex_Y) * rate, 0);
                    var newGeometricCoordinates = new GeometricCoordinates(block.StartGeometricCoordinates.Vertex_X, newVertexY);
                    slotBlock.Add(new TextBlock
                    {
                        Text = block.Text,
                        Paragraphs = block.Paragraphs,
                        StartGeometricCoordinates = newGeometricCoordinates
                    });
                    slotBlockIndexs.Add(slotBlockIndex);
                }
            }
            ++slotBlockIndex;
        }
        var usedSlotBlockIndexs = slotBlockIndexs.OrderByDescending(x => x);
        foreach (var index in usedSlotBlockIndexs)
        {
            adjustedTextBlocks.RemoveAt(index);
        }
        foreach(var block in slotBlock)
        {
            Import_Slot? importedSlot = null;
            int slotNumber = 0;
            if (IdentifySlot(block.Text, out slotNumber))
            {
                importedSlot = new Import_Slot
                {
                    SlotNumber = slotNumber,
                    Vertex_Y = block.StartGeometricCoordinates.Vertex_Y
                };
            }
            else if(IdentifySlotByWord(block, out slotNumber))
            {
                importedSlot = new Import_Slot
                {
                    SlotNumber = slotNumber,
                    Vertex_Y = block.StartGeometricCoordinates.Vertex_Y
                };
            }
            else
            {
                // Hanlde the case that the slot information may stored in 2 blocks next to each other
                var searchTextBlock = adjustedTextBlocks
                    .Where(b => (b.StartGeometricCoordinates.Vertex_Y - 5) <= block.StartGeometricCoordinates.Vertex_Y && 
                                block.StartGeometricCoordinates.Vertex_Y <= (b.StartGeometricCoordinates.Vertex_Y + 5) &&
                                b.StartGeometricCoordinates.Vertex_X <= (block.StartGeometricCoordinates.Vertex_X + 40))
                    .FirstOrDefault();
                // slot => y trong khoảng 2, x trong khoảng 10
                if (IdentifySlot($"{block.Text}{searchTextBlock?.Text}", out slotNumber))
                {
                    importedSlot = new Import_Slot
                    {
                        SlotNumber = slotNumber,
                        Vertex_Y = block.StartGeometricCoordinates.Vertex_Y
                    };
                }
            }

            if(importedSlot is not null)
            {
                slots.Add(importedSlot);
            }
        }

        // Get semester based on startDate and endDate
        var startDate = weeklyDates.MinBy(w => w.Date)?.Date;
        var endDate = weeklyDates.MaxBy(w => w.Date)?.Date;
        TimeOnly timeOnly = new TimeOnly(0, 0);
        if (startDate is null || endDate is null)
        {
            return new Import_Result
            {
                Year = year ?? 0,
                SemesterFound = false,
                DatesCount = weeklyDates.Count,
                SlotsCount = slots.Count,
                Dates = weeklyDates,
                Slots = slots
            };
        }
        var getSemestersResult = await _semesterService.GetAll(1, 10, 10, null, null, startDate.Value.ToDateTime(timeOnly), endDate.Value.ToDateTime(timeOnly));
        if (!getSemestersResult.IsSuccess)
        {
            return new Import_Result
            {
                Year = year ?? 0,
                SemesterFound = false,
                DatesCount = weeklyDates.Count,
                SlotsCount = slots.Count,
                Dates = weeklyDates,
                Slots = slots
            };
        }
        existedSemester = getSemestersResult.Result?.FirstOrDefault();
        if(existedSemester is null)
        {
            return new Import_Result
            {
                Year = year ?? 0,
                SemesterFound = false,
                DatesCount = weeklyDates.Count,
                SlotsCount = slots.Count,
                Dates = weeklyDates,
                Slots = slots
            };
        }


        // Remake schedule data, remove words that seems not to be class code
        foreach (var block in adjustedTextBlocks)
        {
            foreach (var paragraph in block.Paragraphs)
            {
                var words = paragraph.Words.ToList();
                foreach(var word in paragraph.Words)
                {
                    if(word.Text.ToUpper() == "AT" || word.Text.ToUpper() == "ATTENDED" || word.Text == "(" || word.Text == ")")
                    {
                        words.Remove(word);
                    }
                    else if (IsMatchRoomFormat(word.Text))
                    {
                        word.Text = ";;;";
                    }
                }
                paragraph.Words = words;
            }
            block.Text = string.Join("", block.Paragraphs.SelectMany(p => p.Words).Select(w => w.Text));
        }


        // Verify slot information
        var copySlots = slots.ToList();
        foreach (var slot in copySlots)
        {
            var getSlotResult = await _slotService.GetAllSlots(1, 1, 10, slot.SlotNumber, null, null, null);
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

        // Check duplicated slot
        copySlots = slots.ToList();
        foreach (var slot in copySlots)
        {
            var duplicateSlot = slots.Where(s => s.SlotNumber == slot.SlotNumber);
            if(duplicateSlot.Count() > 1)
            {
                // Select the most correct slot
                foreach(var checkSlot in duplicateSlot)
                {
                    var vertexY = checkSlot.Vertex_Y;

                    var bottomPoint = slots.Where(s => s.SlotOrder < checkSlot.SlotOrder);
                    var minY = 0;
                    if(bottomPoint.Count() > 0)
                    {
                        minY = bottomPoint.Select(s => s.Vertex_Y).Max();
                    }

                    var upPoint = slots.Where(s => s.SlotOrder > checkSlot.SlotOrder);
                    var maxY = 0;
                    if(upPoint.Count() > 0)
                    {
                        maxY = upPoint.Select(s => s.Vertex_Y).Min();
                    }

                    if (vertexY <= minY || (maxY != 0 && vertexY > maxY))
                    {
                        // Remove wrong slot
                        slots.Remove(checkSlot);
                        break;
                    }
                }
            }

            var checkDuplicateSlot = slots.Where(s => s.SlotNumber == slot.SlotNumber);
            if(checkDuplicateSlot.Count() > 1)
            {
                var removeSlot = checkDuplicateSlot.ElementAtOrDefault(0);
                if(removeSlot is not null)
                {
                    slots.Remove(removeSlot);
                }
            }
        }


        // No test yet=======================================================
        // Sort for splitting purpose
        weeklyDates = weeklyDates.OrderBy(s => s.Vertex_X).ToList();
        slots = slots.OrderBy(s => s.Vertex_Y).ToList();


        // Lets split a single textBlock contains 2 or more schedules to many textBlocks
        var datesVertex_x_List = weeklyDates.Select(d => d.Vertex_X);
        var copyAdjustedTextBlocks = adjustedTextBlocks.ToList();
        foreach (var block in copyAdjustedTextBlocks)
        {
            var containedDateVertex_x = new List<int>();

            foreach(var vertex_x in datesVertex_x_List)
            {
                if((block.StartGeometricCoordinates.Vertex_X <= vertex_x && vertex_x <= block.EndGeometricCoordinates.Vertex_X) ||
                    ((block.StartGeometricCoordinates.Vertex_X - 10) <= vertex_x && vertex_x <= (block.StartGeometricCoordinates.Vertex_X + 10)) ||
                    ((block.EndGeometricCoordinates.Vertex_X - 10) <= vertex_x && vertex_x <= (block.EndGeometricCoordinates.Vertex_X + 10)))
                {
                    containedDateVertex_x.Add(vertex_x);
                }
            }
            if(containedDateVertex_x.Count > 1)
            {
                //containedDateVertex_x.Reverse();
                var classCode = block.Text.Split(";;;");
                if (classCode.Length == 0) continue;

                int index = 0;
                foreach(var vertex_x in containedDateVertex_x)
                {
                    if(index > classCode.Length - 1)
                    {
                        break;
                    }
                    if (classCode[index] == "-")
                    {
                        continue;
                    }
                    if (classCode[index].Contains(";;;"))
                    {
                        var removedIndex = classCode[index].IndexOf(";;;");
                        classCode[index] = classCode[index].Remove(removedIndex, 3);
                    }
                    adjustedTextBlocks.Add(new TextBlock
                    {
                        Text = classCode[index],
                        StartGeometricCoordinates = new GeometricCoordinates(vertex_x, block.StartGeometricCoordinates.Vertex_Y)
                    });
                    index++;
                }
                adjustedTextBlocks.Remove(block);
            }
        }


        // Lets split a single textBlock contains 2 or more schedules to many textBlocks (but compare to slot -> vertex y)
        var slotVertex_y_list = slots.Select(s => s.Vertex_Y);
        var copyAdjustedTextBlocksForCheckSlot = adjustedTextBlocks.ToList();
        foreach(var block in copyAdjustedTextBlocksForCheckSlot)
        {
            var containedSlotVertex_y = new List<int>();

            foreach(var vertex_y in slotVertex_y_list)
            {
                if ((block.StartGeometricCoordinates.Vertex_Y <= vertex_y && vertex_y <= block.ModifyGeometricCoordinates.Vertex_Y) ||
                    ((block.StartGeometricCoordinates.Vertex_Y - 10) <= vertex_y && vertex_y <= (block.StartGeometricCoordinates.Vertex_Y + 10)) ||
                    ((block.ModifyGeometricCoordinates.Vertex_Y - 10) <= vertex_y && vertex_y <= (block.ModifyGeometricCoordinates.Vertex_Y + 10)))
                {
                    containedSlotVertex_y.Add(vertex_y);
                }
            }

            if (containedSlotVertex_y.Count > 1)
            {
                //IsInValidParagraph
                var validParagraphs = block.Paragraphs.Where(p => !IsInValidParagraph(p.Text)).ToArray();
                int index = 0;
                foreach (var vertex_y in containedSlotVertex_y)
                {
                    if (index > validParagraphs.Length - 1)
                    {
                        break;
                    }
                    adjustedTextBlocks.Add(new TextBlock
                    {
                        Text = validParagraphs[index].Text,
                        StartGeometricCoordinates = new GeometricCoordinates(block.StartGeometricCoordinates.Vertex_X, vertex_y),
                        Paragraphs = new List<Paragraph> { validParagraphs[index] }
                    });
                    index++;
                }
                adjustedTextBlocks.Remove(block);
            }
            else
            {
                var checkVertex_y = containedSlotVertex_y.FirstOrDefault();
                if(checkVertex_y != 0)
                {
                    if (block.StartGeometricCoordinates.Vertex_Y < checkVertex_y && checkVertex_y < block.ModifyGeometricCoordinates.Vertex_Y)
                    {
                        var validParagraphs = block.Paragraphs.Where(p => !IsInValidParagraph(p.Text)).ToArray();
                        adjustedTextBlocks.Add(new TextBlock
                        {
                            Text = string.Concat(validParagraphs.Select(p => p.Text)),
                            StartGeometricCoordinates = new GeometricCoordinates(block.StartGeometricCoordinates.Vertex_X, checkVertex_y),
                            Paragraphs = validParagraphs
                        });
                        adjustedTextBlocks.Remove(block);
                    }
                }
            }
        }


        // Before remove seperator word ";;;"
        /*foreach(var block in adjustedTextBlocks)
        {

        }*/

        // Remove seperator word ";;;"
        foreach (var block in adjustedTextBlocks)
        {
            if (block.Text.Contains(";;;"))
            {
                var removedIndex = block.Text.IndexOf(";;;");
                block.Text = block.Text.Remove(removedIndex, 3);
            }
        }


        // Remove empty text block
        var copyTextBlocks = adjustedTextBlocks.ToList();
        foreach (var block in copyTextBlocks)
        {
            if (block.Text == string.Empty || block.Text == "")
            {
                adjustedTextBlocks.Remove(block);
            }
        }


        // Sort schedules into each slot
        foreach (var block in adjustedTextBlocks)
        {
            var vertex_y = block.StartGeometricCoordinates.Vertex_Y;
            var slot = slots.Where(s => s.CheckVertex_Y(vertex_y, 10)).FirstOrDefault();
            if (slot is not null)
            {
                slot.ClassSlots.Add(new Import_Class
                {
                    ClassCode = block.Text,
                    Vertex_X = block.StartGeometricCoordinates.Vertex_X
                });
            }
        }


        // Validate schedule (class code) information of each slot
        var existedClassCode = await _classService.GetAllClassCodes(existedSemester.SemesterID, lecturerId);
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
            for (int i = 0; i <= totalDates - 1; i++)
            {
                var acceptedClassSlot = slot.ClassSlots.Where(c => c != null && c.CheckVertex_X(weeklyDatesPosition.ElementAt(i), 30)).FirstOrDefault();
                adjustedClassSlots[i] = acceptedClassSlot;
            }
            slot.AdjustedClassSlots = adjustedClassSlots.ToList();
            slot.ClassSlots = Enumerable.Empty<Import_Class>().ToList();
        });

        // Sort
        weeklyDates = weeklyDates.OrderBy(s => s.Date).ToList();
        slots = slots.OrderBy(s => s.SlotNumber).ToList();

        return new Import_Result
        {
            Year = year ?? 0,
            SemesterFound = true,
            SemesterCode = existedSemester.SemesterCode,
            SemesterId = existedSemester.SemesterID,
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

    private bool IdentifySlotByWord(TextBlock block, out int slotNumber)
    {
        slotNumber = 0;
        var containingParagraph = block.Paragraphs.FirstOrDefault(p => p.Words.Any(w => w.Text.ToUpper() == "SLOT"));
        if (containingParagraph is null) return false;
        var words = containingParagraph.Words.ToArray();
        for(int i = 0; i < words.Length - 1; i++)
        {
            if (words[i].Text.ToUpper() == "SLOT")
            {
                if (int.TryParse(words[i + 1].Text, out slotNumber))
                {
                    return true;
                }
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
                if(slot.SlotNumber == 4)
                {
                    Console.WriteLine("Slot 4:" + classSlot.ClassCode.ToUpper());
                }
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

    private bool IsMatchRoomFormat(string input)
    {
        // Define the regex pattern
        string pattern = @"^P\.\d{3}$";

        // Create a regex object
        Regex regex = new Regex(pattern);

        // Check if the input matches the pattern
        return regex.IsMatch(input);
    }

    private bool IsInValidParagraph(string input)
    {
        string result = input.Replace("(", "").Replace(")", "");
        result = result.ToUpper().Replace("NOTYET", "").Trim();
        if(result == "")
        {
            return true;
        }
        string pattern = @"^([0-9]|1[0-9]|2[0-3]):[0-5][0-9]-([0-9]|1[0-9]|2[0-3]):[0-5][0-9]$";
        return Regex.IsMatch(result, pattern);
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
    public string SemesterCode { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public bool SemesterFound { get; set; }
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
    public GeometricCoordinates ModifyGeometricCoordinates { get; set; } = new GeometricCoordinates(0, 0);
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