﻿using System.Collections.ObjectModel;
using AuthoringTool.PresentationLogic.LearningElement;
using AuthoringTool.PresentationLogic.LearningSpace;
using AuthoringTool.View.LearningWorld;

namespace AuthoringTool.PresentationLogic.LearningWorld;

public class LearningWorldViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LearningWorldViewModel"/> class.
    /// </summary>
    /// <param name="name">The name of the learning world.</param>
    /// <param name="shortname">The short name (abbreviation) of the learning world.</param>
    /// <param name="authors">The string containing the names of all the authors working on the learning world.</param>
    /// <param name="language">The primary language used in this learning world.</param>
    /// <param name="description">A description of the learning world and its contents.</param>
    /// <param name="goals">A description of the goals this learning world is supposed to achieve.</param>
    /// <param name="learningElements">Optional collection of learning elements contained in the learning world.
    /// Should be used when loading a saved learning world into the application.</param>
    /// <param name="learningSpaces">Optional collection of learning spaces contained in the learning world.
    /// Should be used when loading a saved learnign world into the application.</param>
    public LearningWorldViewModel(string name, string shortname, string authors, string language, string description,
        string goals, ICollection<LearningElementViewModel>? learningElements = null,
        ICollection<LearningSpaceViewModel>? learningSpaces = null)
    {
        Name = name;
        Shortname = shortname;
        Authors = authors;
        Language = language;
        Description = description;
        Goals = goals;
        LearningElements = learningElements ?? new Collection<LearningElementViewModel>();
        LearningSpaces = learningSpaces ?? new Collection<LearningSpaceViewModel>();
    }
       
    public ICollection<LearningElementViewModel> LearningElements { get; set; }
    public ICollection<LearningSpaceViewModel> LearningSpaces { get; set; }
    public string Name { get; set; }
    public string Shortname { get; set; }
    public string Authors { get; set; }
    public string Language { get; set; }
    public string Description { get; set; }
    public string Goals { get; set; }
}