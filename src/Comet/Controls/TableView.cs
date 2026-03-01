using System;
using System.Collections.Generic;

namespace Comet
{
/// <summary>
/// Table view for form-style layouts with sections.
/// </summary>
public class TableView : View
{
public List<TableSection> Root { get; set; } = new List<TableSection>();

public TableView() { }

public TableView(params TableSection[] sections)
{
Root.AddRange(sections);
}
}

/// <summary>
/// A section within a TableView containing cells.
/// </summary>
public class TableSection
{
public string Title { get; set; }
public List<View> Cells { get; set; } = new List<View>();

public TableSection() { }

public TableSection(string title, params View[] cells)
{
Title = title;
Cells.AddRange(cells);
}
}

/// <summary>
/// A text cell for use within TableSection.
/// </summary>
public class TextCell : View
{
private Binding<string> _text;
public Binding<string> CellText
{
get => _text;
set => this.SetBindingValue(ref _text, value);
}

private Binding<string> _detail;
public Binding<string> Detail
{
get => _detail;
set => this.SetBindingValue(ref _detail, value);
}

public Action OnTapped { get; set; }
}

/// <summary>
/// A switch cell for toggle items within TableSection.
/// </summary>
public class SwitchCell : View
{
private Binding<string> _text;
public Binding<string> CellText
{
get => _text;
set => this.SetBindingValue(ref _text, value);
}

private Binding<bool> _on;
public Binding<bool> On
{
get => _on;
set => this.SetBindingValue(ref _on, value);
}
}

/// <summary>
/// An entry cell for text input within TableSection.
/// </summary>
public class EntryCell : View
{
private Binding<string> _label;
public Binding<string> Label
{
get => _label;
set => this.SetBindingValue(ref _label, value);
}

private Binding<string> _text;
public Binding<string> CellText
{
get => _text;
set => this.SetBindingValue(ref _text, value);
}

private Binding<string> _placeholder;
public Binding<string> Placeholder
{
get => _placeholder;
set => this.SetBindingValue(ref _placeholder, value);
}
}
}
