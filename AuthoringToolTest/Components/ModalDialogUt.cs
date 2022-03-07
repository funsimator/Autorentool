using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AuthoringTool.Components;
using NUnit.Framework;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace AuthoringToolTest.Components;

[TestFixture]
public class ModalDialogUt
{
    [Test]
    public void ModalDialog_StandardConstructor_AllPropertiesInitialized()
    {
        using var ctx = new Bunit.TestContext();
        var title = "Test Dialog";
        var text = "This is a dialog for automated testing purposes";
        Action<Tuple<ModalDialog.ModalDialogReturnValue, IDictionary<string, string>?>> onClose = _ => { };
        var dialogType = ModalDialog.ModalDialogType.Ok;
        var inputFields = new List<ModalDialog.ModalDialogInputField>
        {
            new("Test1", ModalDialog.ModalDialogInputType.Text, false),
            new ModalDialog.ModalDialogDropdownInputField("Test2",
                new[] { new ModalDialog.ModalDialogDropdownInputFieldChoiceMapping(null, new[] { "Test1", "Test2" }) },
                true),
        };

        var systemUnderTest = CreateRenderedModalDialogComponent(ctx, title, text, onClose,
            dialogType, inputFields);
        
        Assert.AreEqual(title, systemUnderTest.Instance.Title);
        Assert.AreEqual(text, systemUnderTest.Instance.Text);
        //we need to construct an EventCallback from our action
        Assert.AreEqual(EventCallback.Factory.Create(onClose.Target ??
                                                     throw new InvalidOperationException("onClose.Target is null"),
                onClose),
            systemUnderTest.Instance.OnClose);
        Assert.AreEqual(dialogType, systemUnderTest.Instance.DialogType);
        Assert.AreEqual(inputFields, systemUnderTest.Instance.InputFields);
    }

    //ModalDialogType, css selector string, ModalDialogReturnValue
    static object[] ModalDialog_ButtonClicked_CallsCallbackWithCorrectDialogReturnValue_TestCaseSource =
    {
        new object[] { ModalDialog.ModalDialogType.Ok, "#btn-ok", ModalDialog.ModalDialogReturnValue.Ok },
        new object[] { ModalDialog.ModalDialogType.OkCancel, "#btn-ok", ModalDialog.ModalDialogReturnValue.Ok },
        new object[] { ModalDialog.ModalDialogType.OkCancel, "#btn-cancel", ModalDialog.ModalDialogReturnValue.Cancel },
        new object[] { ModalDialog.ModalDialogType.DeleteCancel, "#btn-delete", ModalDialog.ModalDialogReturnValue.Delete },
        new object[] { ModalDialog.ModalDialogType.DeleteCancel, "#btn-cancel", ModalDialog.ModalDialogReturnValue.Cancel },
        new object[] { ModalDialog.ModalDialogType.YesNoCancel, "#btn-yes", ModalDialog.ModalDialogReturnValue.Yes },
        new object[] { ModalDialog.ModalDialogType.YesNoCancel, "#btn-no", ModalDialog.ModalDialogReturnValue.No },
        new object[] { ModalDialog.ModalDialogType.YesNoCancel, "#btn-cancel", ModalDialog.ModalDialogReturnValue.Cancel },
    };
    [Test]
    [TestCaseSource(nameof(ModalDialog_ButtonClicked_CallsCallbackWithCorrectDialogReturnValue_TestCaseSource))]
    public void ModalDialog_ButtonClicked_CallsCallbackWithCorrectDialogReturnValue(ModalDialog.ModalDialogType type,
        string cssSelector, ModalDialog.ModalDialogReturnValue returnValue)
    {
        using var ctx = new Bunit.TestContext();
        
        //ModalDialogType.Ok
        //Ok button
        var title = "Test Dialog";
        var text = "This is a dialog for automated testing purposes";
        var onCloseCalled = false;
        Action<Tuple<ModalDialog.ModalDialogReturnValue, IDictionary<string, string>?>> onClose = tuple =>
        {
            Assert.AreEqual(returnValue, tuple.Item1);
            Assert.AreEqual(null, tuple.Item2);
            onCloseCalled = true;
        };
        var dialogType = type;

        var systemUnderTest = CreateRenderedModalDialogComponent(ctx, title, text, onClose,
            dialogType, null, null);
        var btn = systemUnderTest.Find(cssSelector);
        btn.Click();
        Assert.AreEqual(true, onCloseCalled);
    }
    
    [Test]
    public void ModalDialog_EnterKeyPressedOnInputField_SubmitsDialog()
    {
        using var ctx = new Bunit.TestContext();
        
        var title = "Test Dialog";
        var text = "This is a dialog for automated testing purposes";
        var onCloseCalled = false;
        Action<Tuple<ModalDialog.ModalDialogReturnValue, IDictionary<string, string>?>> onClose = tuple =>
        {
            Assert.AreEqual(ModalDialog.ModalDialogReturnValue.Ok, tuple.Item1);
            Assert.AreEqual("foobar", tuple.Item2!["Test1"]);
            onCloseCalled = true;
        };
        var dialogType = ModalDialog.ModalDialogType.Ok;
        var inputFields = new List<ModalDialog.ModalDialogInputField>
        {
            new("Test1", ModalDialog.ModalDialogInputType.Text, true),
        };

        var systemUnderTest = CreateRenderedModalDialogComponent(ctx, title, text, onClose,
            dialogType, inputFields, null);

        var element = systemUnderTest.Find("#modal-input-field-test1");
        element.Input("foobar");
        element.KeyDown(Key.Enter);

        Assert.AreEqual(true, onCloseCalled);
    }
    
    [Test]
    public void ModalDialog_DialogSubmitted_CallsCallbackWithCorrectInputFieldValues()
    {
        using var ctx = new Bunit.TestContext();
        
        var title = "Test Dialog";
        var text = "This is a dialog for automated testing purposes";
        var onCloseCalled = false;
        Action<Tuple<ModalDialog.ModalDialogReturnValue, IDictionary<string, string>?>> onClose = tuple =>
        {
            var (returnValue, dictionary) = tuple;
            Assert.AreEqual(ModalDialog.ModalDialogReturnValue.Ok, returnValue);
            Assert.NotNull(dictionary);
            
            Assert.AreEqual("foobar123", dictionary!["Test1"]);
            Assert.AreEqual("Bar", dictionary["Test2"]);
            Assert.AreEqual("Baz", dictionary["Test3"]);
            Assert.AreEqual("", dictionary["Test4"]);
            onCloseCalled = true;
        };
        var dialogType = ModalDialog.ModalDialogType.Ok;
        var inputFields = new List<ModalDialog.ModalDialogInputField>
        {
            new("Test1", ModalDialog.ModalDialogInputType.Text, true),
            new ModalDialog.ModalDialogDropdownInputField("Test2",
                new[] { new ModalDialog.ModalDialogDropdownInputFieldChoiceMapping(null, new[] { "Foo", "Bar" }) },
                true),
            new ModalDialog.ModalDialogDropdownInputField("Test3",
                new[] { new ModalDialog.ModalDialogDropdownInputFieldChoiceMapping(new Dictionary<string, string>
                {
                    {"Test2", "Bar"}
                }, new[] { "Foz", "Baz" }) },
                true),
            new("Test4", ModalDialog.ModalDialogInputType.Text)
        };

        var systemUnderTest = CreateRenderedModalDialogComponent(ctx, title, text, onClose,
            dialogType, inputFields);
        systemUnderTest.Find("#modal-input-field-test1").Input("foobar123");
        systemUnderTest.Find("#modal-input-drop-test2").Change("Bar");
        systemUnderTest.Find("#modal-input-drop-test3").Change("Baz");
        
        systemUnderTest.Find("#btn-ok").Click();
        
        Assert.AreEqual(true, onCloseCalled);
    }

    [Test]
    public void ModalDialog_MissingRequiredValues_RefusesToCallCallback()
    {
        using var ctx = new Bunit.TestContext();
        
        var title = "Test Dialog";
        var text = "This is a dialog for automated testing purposes";
        Action<Tuple<ModalDialog.ModalDialogReturnValue, IDictionary<string, string>?>> onClose = tuple =>
        {
            Assert.Fail("onClose was called, but should not have.");
        };
        var dialogType = ModalDialog.ModalDialogType.Ok;
        var inputFields = new List<ModalDialog.ModalDialogInputField>
        {
            new("Test1", ModalDialog.ModalDialogInputType.Text, true),
            new ModalDialog.ModalDialogDropdownInputField("Test2",
                new[] { new ModalDialog.ModalDialogDropdownInputFieldChoiceMapping(null, new[] { "Foo", "Bar" }) },
                true),
            new ModalDialog.ModalDialogDropdownInputField("Test3",
                new[] { new ModalDialog.ModalDialogDropdownInputFieldChoiceMapping(new Dictionary<string, string>
                {
                    {"Test2", "Bar"}
                }, new[] { "Foz", "Baz" }) },
                true),
            new("Test4", ModalDialog.ModalDialogInputType.Text)
        };

        var systemUnderTest = CreateRenderedModalDialogComponent(ctx, title, text, onClose,
            dialogType, inputFields);
        
        systemUnderTest.Find("#btn-ok").Click();
        Assert.DoesNotThrow(() => systemUnderTest.Find(".modal-input-warning"));
        systemUnderTest.Find("#modal-input-field-test1").Input("foobar123");
        Assert.DoesNotThrow(() => systemUnderTest.Find(".modal-input-warning"));
        systemUnderTest.Find("#modal-input-drop-test2").Change("Bar");
        Assert.DoesNotThrow(() => systemUnderTest.Find(".modal-input-warning"));
        systemUnderTest.Find("#modal-input-drop-test3").Change("Baz");
        Assert.Throws<ElementNotFoundException>(() => systemUnderTest.Find(".modal-input-warning"));
    }

    [Test]
    public void ModalDialog_DropdownSelectionRules_ChangeAvailableOptions()
    {
        using var ctx = new Bunit.TestContext();
        
        var title = "Test Dialog";
        var text = "This is a dialog for automated testing purposes";
        Action<Tuple<ModalDialog.ModalDialogReturnValue, IDictionary<string, string>?>> onClose = tuple =>
        {
            Assert.Fail("onClose was called, but should not have.");
        };
        var dialogType = ModalDialog.ModalDialogType.Ok;
        var inputFields = new List<ModalDialog.ModalDialogInputField>
        {
            new("Test1", ModalDialog.ModalDialogInputType.Text, true),
            new ModalDialog.ModalDialogDropdownInputField("Test2",
                new[] { new ModalDialog.ModalDialogDropdownInputFieldChoiceMapping(null, new[] { "Foo", "Bar" }) },
                true),
            new ModalDialog.ModalDialogDropdownInputField("Test3",
                new[] { new ModalDialog.ModalDialogDropdownInputFieldChoiceMapping(new Dictionary<string, string>
                {
                    {"Test2", "Bar"}
                }, new[] { "Foz", "Baz" }) },
                true),
            new("Test4", ModalDialog.ModalDialogInputType.Text)
        };

        var systemUnderTest = CreateRenderedModalDialogComponent(ctx, title, text, onClose,
            dialogType, inputFields);
        
        Assert.Throws<ElementNotFoundException>(() => systemUnderTest.Find("#modal-input-drop-test3-foz"));
        Assert.Throws<ElementNotFoundException>(() => systemUnderTest.Find("#modal-input-drop-test3-baz"));
        
        systemUnderTest.Find("#modal-input-drop-test2").Change("Bar");
        
        Assert.DoesNotThrow(() => systemUnderTest.Find("#modal-input-drop-test3-foz"));
        Assert.DoesNotThrow(() => systemUnderTest.Find("#modal-input-drop-test3-baz"));
    }

    [Test]
    public void ModalDialog_InitialValues_PopulatedAndReturned()
    {
        using var ctx = new Bunit.TestContext();
        
        var title = "Test Dialog";
        var text = "This is a dialog for automated testing purposes";
        var onCloseCalled = false;
        Action<Tuple<ModalDialog.ModalDialogReturnValue, IDictionary<string, string>?>> onClose = tuple =>
        {
            var (returnValue, dictionary) = tuple;
            Assert.AreEqual(ModalDialog.ModalDialogReturnValue.Ok, returnValue);
            Assert.NotNull(dictionary);
            
            Assert.AreEqual("foobar", dictionary["Test1"]);
            Assert.AreEqual("Bar", dictionary["Test2"]);
            Assert.AreEqual("", dictionary["Test3"]);
            onCloseCalled = true;
        };
        var dialogType = ModalDialog.ModalDialogType.Ok;
        var inputFields = new List<ModalDialog.ModalDialogInputField>
        {
            new("Test1", ModalDialog.ModalDialogInputType.Text, true),
            new ModalDialog.ModalDialogDropdownInputField("Test2",
                new[] { new ModalDialog.ModalDialogDropdownInputFieldChoiceMapping(null, new[] { "Foo", "Bar" }) },
                true),
            new("Test3", ModalDialog.ModalDialogInputType.Text)
        };
        var inputFieldsInitialValues = new Dictionary<string, string>
        {
            {"Test1", "foobar"},
            {"Test2", "Bar"}
        };

        var systemUnderTest = CreateRenderedModalDialogComponent(ctx, title, text, onClose,
            dialogType, inputFields, inputFieldsInitialValues);
        Assert.AreEqual("foobar", systemUnderTest.Find("#modal-input-field-test1").Attributes["value"]?.Value);
        Assert.AreEqual("Bar", systemUnderTest.Find("#modal-input-drop-test2").Attributes["value"]?.Value);
        Assert.AreEqual("", systemUnderTest.Find("#modal-input-field-test3").Attributes["value"]?.Value);

        systemUnderTest.Find("#btn-ok").Click();
        Assert.AreEqual(true, onCloseCalled);
    }

    [Test]
    public void ModalDialog_InitialValue_ReturnsCorrectValueAfterChange()
    {
        using var ctx = new Bunit.TestContext();
        
        var title = "Test Dialog";
        var text = "This is a dialog for automated testing purposes";
        var onCloseCalled = false;
        Action<Tuple<ModalDialog.ModalDialogReturnValue, IDictionary<string, string>?>> onClose = tuple =>
        {
            var (returnValue, dictionary) = tuple;
            Assert.AreEqual(ModalDialog.ModalDialogReturnValue.Ok, returnValue);
            Assert.NotNull(dictionary);
            
            Assert.AreEqual("boofar", dictionary["Test1"]);
            Assert.AreEqual("Foo", dictionary["Test2"]);
            Assert.AreEqual("foobar123", dictionary["Test3"]);
            onCloseCalled = true;
        };
        var dialogType = ModalDialog.ModalDialogType.Ok;
        var inputFields = new List<ModalDialog.ModalDialogInputField>
        {
            new("Test1", ModalDialog.ModalDialogInputType.Text, true),
            new ModalDialog.ModalDialogDropdownInputField("Test2",
                new[] { new ModalDialog.ModalDialogDropdownInputFieldChoiceMapping(null, new[] { "Foo", "Bar" }) },
                true),
            new("Test3", ModalDialog.ModalDialogInputType.Text)
        };
        var inputFieldsInitialValues = new Dictionary<string, string>
        {
            {"Test1", "foobar"},
            {"Test2", "Bar"}
        };

        var systemUnderTest = CreateRenderedModalDialogComponent(ctx, title, text, onClose,
            dialogType, inputFields, inputFieldsInitialValues);
        systemUnderTest.Find("#modal-input-field-test1").Input("boofar");
        systemUnderTest.Find("#modal-input-drop-test2").Change("Foo");
        systemUnderTest.Find("#modal-input-field-test3").Input("foobar123");

        systemUnderTest.Find("#btn-ok").Click();
        Assert.AreEqual(true, onCloseCalled);
    }

    [Test]
    public void ModalDialog_DropdownSelectionRules_CorrectWhenInitialValuesProvided()
    {
        using var ctx = new Bunit.TestContext();
        
        var title = "Test Dialog";
        var text = "This is a dialog for automated testing purposes";
        Action<Tuple<ModalDialog.ModalDialogReturnValue, IDictionary<string, string>?>> onClose = tuple =>
        {
            Assert.Fail("onclose unexpectedly called");
        };
        var dialogType = ModalDialog.ModalDialogType.Ok;
        var inputFields = new List<ModalDialog.ModalDialogInputField>
        {
            new ModalDialog.ModalDialogDropdownInputField("Test1",
                new[] { new ModalDialog.ModalDialogDropdownInputFieldChoiceMapping(null, new[] { "Foo", "Bar" }) },
                true),
            new ModalDialog.ModalDialogDropdownInputField("Test2",
                new[] { new ModalDialog.ModalDialogDropdownInputFieldChoiceMapping(new Dictionary<string, string>
                {
                    {"Test1", "Bar"}
                }, new[] { "Foz", "Baz" }) },
                true),
        };
        var inputFieldsInitialValues = new Dictionary<string, string>
        {
            {"Test1", "Bar"},
            {"Test2", "Baz"}
        };

        var systemUnderTest = CreateRenderedModalDialogComponent(ctx, title, text, onClose,
            dialogType, inputFields, inputFieldsInitialValues);

        Assert.AreEqual("Bar", systemUnderTest.Find("#modal-input-drop-test1").Attributes["value"]?.Value);
        Assert.AreEqual("Baz", systemUnderTest.Find("#modal-input-drop-test2").Attributes["value"]?.Value);
        Assert.DoesNotThrow(() => systemUnderTest.Find("#modal-input-drop-test2-foz"));
        Assert.DoesNotThrow(() => systemUnderTest.Find("#modal-input-drop-test2-baz"));
    }
    
    private IRenderedComponent<ModalDialog> CreateRenderedModalDialogComponent(Bunit.TestContext ctx, string title, string text,
        Action<Tuple<ModalDialog.ModalDialogReturnValue, IDictionary<string, string>?>> onClose,
        ModalDialog.ModalDialogType dialogType,
        IEnumerable<ModalDialog.ModalDialogInputField>? inputFields = null, IDictionary<string,string>? initialValues = null)
    {
        return ctx.RenderComponent<ModalDialog>(parameters => parameters
            .Add(p => p.Title, title)
            .Add(p => p.Text, text)
            .Add(p => p.OnClose, onClose)
            .Add(p => p.DialogType, dialogType)
            .Add(p => p.InputFields, inputFields)
            .Add(p => p.InputFieldsInitialValue, initialValues));
    }
}