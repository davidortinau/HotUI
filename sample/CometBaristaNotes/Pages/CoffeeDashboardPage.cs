using System;
using System.Collections.Generic;
using System.Linq;
using Comet;
using CometBaristaNotes.Components;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;

namespace CometBaristaNotes.Pages;

public class CoffeeDashboardState
{
public int SelectedBeanIndex { get; set; }

public bool IncludeCompletedBags { get; set; }
}

public class CoffeeDashboardPage : Component<CoffeeDashboardState>
{
readonly IDataStore _store;
readonly IDataChangeNotifier? _notifier;

public CoffeeDashboardPage()
{
_store = IPlatformApplication.Current?.Services.GetService<IDataStore>() ?? (IDataStore)InMemoryDataStore.Instance;
_notifier = IPlatformApplication.Current?.Services.GetService<IDataChangeNotifier>();
if (_notifier != null)
{
_notifier.DataChanged += OnDataChanged;
}
}

public override View Render()
{
var beans = _store.GetAllBeans().OrderBy(bean => bean.Name).ToList();
var selectedIndex = Math.Clamp(State.SelectedBeanIndex, 0, beans.Count);
var selectedBean = selectedIndex == 0 ? null : beans[selectedIndex - 1];
var beanOptions = new[] { "All beans" }
.Concat(beans.Select(bean => $"{bean.Name} • {bean.Roaster ?? "Independent"}"))
.ToArray();

var allShots = selectedBean == null
? _store.GetAllShots()
: _store.GetShotsByBean(selectedBean.Id);
var bags = selectedBean == null
? _store.GetAllBags()
: _store.GetBagsForBean(selectedBean.Id);
if (!State.IncludeCompletedBags)
{
bags = bags.Where(bag => !bag.IsComplete).ToList();
}

var recentShots = allShots.Take(3).ToList();
var ratedShots = allShots.Where(shot => shot.Rating.HasValue).ToList();
var averageRating = ratedShots.Count > 0 ? ratedShots.Average(shot => shot.Rating!.Value) : 0;

var content = VStack(Theme.SpacingM,
BuildHeroCard(selectedBean, allShots.Count, bags.Count(bag => !bag.IsComplete)),
BuildFilterCard(beanOptions, selectedIndex),
BuildMetricsCard(allShots.Count, bags.Count, averageRating, ratedShots.Count),
BuildBagSection(bags),
BuildRecentShotsSection(recentShots),
BuildQuickActions());

return ScrollView(
content.Padding(new Thickness(Theme.SpacingM)))
.Background(Theme.Background)
.Title("Coffee Lab");
}

protected override void OnWillUnmount()
{
if (_notifier != null)
{
_notifier.DataChanged -= OnDataChanged;
}

base.OnWillUnmount();
}

void OnDataChanged(string entityType, int entityId, DataChangeType changeType)
{
MainThread.BeginInvokeOnMainThread(() => Reload());
}

View BuildHeroCard(Bean? selectedBean, int totalShots, int openBags)
{
var subtitle = selectedBean == null
? "A component-driven overview of beans, bags, and recent brews."
: $"{selectedBean.Name} • {selectedBean.Roaster ?? "Independent roaster"}";

return FormHelpers.MakeCard(
VStack(Theme.SpacingS,
Text("Coffee Lab")
.FontFamily(Theme.FontSemibold)
.FontSize(26)
.FontWeight(FontWeight.Bold)
.Color(Theme.TextPrimary),

Text(subtitle)
.FontFamily(Theme.FontRegular)
.FontSize(15)
.Color(Theme.TextSecondary),

Text($"{totalShots} shots tracked • {openBags} bags in rotation")
.FontFamily(Theme.FontSemibold)
.FontSize(14)
.Color(Theme.Primary)));
}

View BuildFilterCard(string[] beanOptions, int selectedIndex)
{
return FormHelpers.MakeCard(
VStack(Theme.SpacingS,
Text("Typed state filters")
.FontFamily(Theme.FontSemibold)
.FontSize(18)
.FontWeight(FontWeight.Bold)
.Color(Theme.TextPrimary),

Text("These controls mutate CoffeeDashboardState via SetState(...).")
.FontFamily(Theme.FontRegular)
.FontSize(13)
.Color(Theme.TextSecondary),

FormHelpers.MakeFormPicker("Bean focus", selectedIndex, beanOptions,
index => SetState(state => state.SelectedBeanIndex = index)),

HStack(Theme.SpacingS,
Toggle(State.IncludeCompletedBags)
.OnColor(Theme.Primary)
.OnToggled(isOn => SetState(state => state.IncludeCompletedBags = isOn)),

Text(State.IncludeCompletedBags
? "Including completed bags"
: "Showing active bags only")
.FontFamily(Theme.FontRegular)
.FontSize(14)
.Color(Theme.TextSecondary)
.VerticalTextAlignment(TextAlignment.Center))));
}

View BuildMetricsCard(int totalShots, int bagCount, double averageRating, int ratedCount)
{
return FormHelpers.MakeCard(
HStack(Theme.SpacingS,
BuildMetric("Shots", totalShots.ToString()),
BuildMetric("Bags", bagCount.ToString()),
BuildMetric("Avg", ratedCount > 0 ? $"{averageRating:F1}★" : "—")));
}

View BuildMetric(string label, string value)
{
return Border(
VStack(4,
Text(value)
.FontFamily(Theme.FontSemibold)
.FontWeight(FontWeight.Bold)
.FontSize(22)
.Color(Theme.TextPrimary)
.HorizontalTextAlignment(TextAlignment.Center),

Text(label)
.FontFamily(Theme.FontRegular)
.FontSize(12)
.Color(Theme.TextMuted)
.HorizontalTextAlignment(TextAlignment.Center)))
.Background(Theme.SurfaceVariant)
.StrokeThickness(0)
.CornerRadius(Theme.RadiusCard)
.Padding(new Thickness(Theme.SpacingM))
.FillHorizontal();
}

View BuildBagSection(List<Bag> bags)
{
var stack = VStack(Theme.SpacingS);
stack.Add(FormHelpers.MakeSectionHeader("Beans to dial in"));

if (bags.Count == 0)
{
stack.Add(FormHelpers.MakeEmptyState(Icons.Coffee, "No bags match this filter", "Adjust the bean focus or include completed bags."));
return stack;
}

foreach (var bag in bags.Take(3))
{
var detail = bag.AverageRating is double average
? $"{bag.ShotCount} shots • {average:F1}★ avg"
: $"{bag.ShotCount} shots • no ratings yet";
stack.Add(FormHelpers.MakeListCard(
bag.BeanName ?? "Unknown bean",
$"Roasted {bag.RoastDate:MMM d}",
detail,
() => Navigation?.Navigate<CoffeeBeanDetailPage>(new CoffeeBeanDetailProps
{
BeanId = bag.BeanId,
Source = "Coffee Lab",
})));
}

return stack;
}

View BuildRecentShotsSection(List<ShotRecord> recentShots)
{
var stack = VStack(Theme.SpacingS);
stack.Add(FormHelpers.MakeSectionHeader("Recent shots"));

if (recentShots.Count == 0)
{
stack.Add(FormHelpers.MakeEmptyState(Icons.Feed, "No shots yet", "Log a new shot to seed the activity feed."));
return stack;
}

foreach (var shot in recentShots)
{
var detail = $"{shot.DoseIn:0.#}g in • {(shot.ActualOutput ?? shot.ExpectedOutput):0.#}g out • {shot.Timestamp:MMM d, h:mm tt}";
stack.Add(FormHelpers.MakeListCard(
shot.BeanName ?? shot.BagDisplayName ?? "Coffee shot",
shot.DrinkType,
detail,
() => Navigation?.Navigate(new ShotLoggingPage(shot.Id))));
}

return stack;
}

View BuildQuickActions()
{
return FormHelpers.MakeCard(
VStack(Theme.SpacingS,
Text("Quick actions")
.FontFamily(Theme.FontSemibold)
.FontSize(18)
.FontWeight(FontWeight.Bold)
.Color(Theme.TextPrimary),

Text("Use typed navigation from the dashboard to jump into detail pages.")
.FontFamily(Theme.FontRegular)
.FontSize(13)
.Color(Theme.TextSecondary),

FormHelpers.MakePrimaryButton("Log a new shot", () => Navigation?.Navigate<ShotLoggingPage>()),
FormHelpers.MakeSecondaryButton("Open activity feed", () => Navigation?.Navigate<ActivityFeedPage>())));
}
}
