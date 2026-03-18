using CometBaristaNotes.Components;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;

namespace CometBaristaNotes.Pages;

public class CoffeeBeanDetailState
{
public bool ShowRecentShots { get; set; } = true;
}

public class CoffeeBeanDetailProps
{
public int BeanId { get; set; }

public string Source { get; set; } = "Coffee Lab";
}

public class CoffeeBeanDetailPage : Component<CoffeeBeanDetailState, CoffeeBeanDetailProps>
{
readonly IDataStore _store;
readonly IRatingService _ratings;

public CoffeeBeanDetailPage()
{
_store = IPlatformApplication.Current?.Services.GetService<IDataStore>() ?? (IDataStore)InMemoryDataStore.Instance;
_ratings = IPlatformApplication.Current?.Services.GetService<IRatingService>() ?? _store;
}

public override View Render()
{
var bean = _store.GetBean(Props.BeanId);
if (bean == null)
{
return ScrollView(
FormHelpers.MakeEmptyState(Icons.Error, "Bean not found", "The selected bean could not be loaded.")
.Padding(new Thickness(Theme.SpacingM))
)
.Background(Theme.Background)
.Title("Coffee Detail");
}

var bags = _store.GetBagsForBean(bean.Id);
var shots = _store.GetShotsByBean(bean.Id).Take(4).ToList();
var rating = _ratings.GetBeanRating(bean.Id);

var content = VStack(Theme.SpacingM,
BuildHero(bean, bags.Count),
RatingDisplayFactory.Create(rating),
BuildBagSection(bags),
BuildShotSection(shots),
BuildActions()
);

return ScrollView(
content.Padding(new Thickness(Theme.SpacingM))
)
.Background(Theme.Background)
.Title(bean.Name);
}

protected override bool ShouldUpdate(CoffeeBeanDetailProps oldProps, CoffeeBeanDetailProps newProps)
=> oldProps?.BeanId != newProps?.BeanId || oldProps?.Source != newProps?.Source;

View BuildHero(Bean bean, int bagCount)
{
return FormHelpers.MakeCard(
VStack(Theme.SpacingS,
Text(bean.Name)
.FontFamily(Theme.FontSemibold)
.FontSize(24)
.FontWeight(FontWeight.Bold)
.Color(Theme.TextPrimary),

Text($"{bean.Roaster ?? "Independent roaster"} • {bean.Origin ?? "Origin pending"}")
.FontFamily(Theme.FontRegular)
.FontSize(15)
.Color(Theme.TextSecondary),

Text($"{bagCount} bag(s) tracked • opened from {Props.Source}")
.FontFamily(Theme.FontSemibold)
.FontSize(13)
.Color(Theme.Primary),

Text(string.IsNullOrWhiteSpace(bean.Notes) ? "No tasting notes captured yet." : bean.Notes!)
.FontFamily(Theme.FontRegular)
.FontSize(14)
.Color(Theme.TextPrimary)
));
}

View BuildBagSection(List<Bag> bags)
{
var stack = VStack(Theme.SpacingS,
FormHelpers.MakeSectionHeader("Bags")
);

if (bags.Count == 0)
{
stack.Add(FormHelpers.MakeEmptyState(Icons.Coffee, "No bags", "Add a bag to start tracking brew performance."));
return stack;
}

foreach (var bag in bags)
{
var detail = bag.AverageRating is double average
? $"{bag.ShotCount} shots • {average:F1}★ avg"
: $"{bag.ShotCount} shots • no ratings yet";
stack.Add(FormHelpers.MakeListCard(
$"Roasted {bag.RoastDate:MMM d}",
bag.IsComplete ? "Completed bag" : "Active bag",
detail,
null));
}

return stack;
}

View BuildShotSection(List<ShotRecord> shots)
{
var stack = VStack(Theme.SpacingS,
FormHelpers.MakeSectionHeader("Recent shots"),

HStack(Theme.SpacingS,
Toggle(State.ShowRecentShots)
.OnColor(Theme.Primary)
.OnToggled(show => SetState(state => state.ShowRecentShots = show)),

Text(State.ShowRecentShots ? "Showing recent shot cards" : "Recent shots hidden")
.FontFamily(Theme.FontRegular)
.FontSize(14)
.Color(Theme.TextSecondary)
.VerticalTextAlignment(TextAlignment.Center)
)
);

if (!State.ShowRecentShots)
{
return stack;
}

if (shots.Count == 0)
{
stack.Add(FormHelpers.MakeEmptyState(Icons.Feed, "No shots yet", "Log a new shot to create a tasting history for this bean."));
return stack;
}

foreach (var shot in shots)
{
var detail = $"{shot.DoseIn:0.#}g in • {(shot.ActualOutput ?? shot.ExpectedOutput):0.#}g out • {shot.Timestamp:MMM d, h:mm tt}";
stack.Add(FormHelpers.MakeListCard(
shot.DrinkType,
shot.MachineName ?? "Machine pending",
detail,
() => Navigation?.Navigate(new ShotLoggingPage(shot.Id))));
}

return stack;
}

View BuildActions()
{
return FormHelpers.MakeCard(
VStack(Theme.SpacingS,
Text("Next moves")
.FontFamily(Theme.FontSemibold)
.FontSize(18)
.FontWeight(FontWeight.Bold)
.Color(Theme.TextPrimary),

Text("This page receives CoffeeBeanDetailProps through typed navigation.")
.FontFamily(Theme.FontRegular)
.FontSize(13)
.Color(Theme.TextSecondary),

FormHelpers.MakePrimaryButton("Log another shot", () => Navigation?.Navigate<ShotLoggingPage>()),
FormHelpers.MakeSecondaryButton("Back to dashboard", () => Navigation?.Pop())
));
}
}
