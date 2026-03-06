using System.Collections.ObjectModel;
using Comet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CometBaristaNotes.Models;
using CometBaristaNotes.Services;
using CometBaristaNotes.Components;
using UXDivers.Popups.Services;

using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiBoxView = Microsoft.Maui.Controls.BoxView;
using MauiButton = Microsoft.Maui.Controls.Button;
using MauiCollectionView = Microsoft.Maui.Controls.CollectionView;
using MauiFontAttributes = Microsoft.Maui.Controls.FontAttributes;

namespace CometBaristaNotes.Pages;

public class ActivityFeedPage : Comet.View
{
	const int _pageSize = 50;
	int _currentPage;
	int _totalShotCount;
	int _filteredShotCount;
	bool _hasMorePages = true;
	readonly ObservableCollection<ShotRecord> _displayedShots = new();
	readonly ShotFilterCriteria _filters = new();

	[State] readonly State<bool> _isLoading = new(true);
	[State] readonly State<string> _errorMessage = new("");
	[State] readonly State<int> _filterVersion = new(0);

	public ActivityFeedPage()
	{
		var notifier = IPlatformApplication.Current?.Services.GetService<IDataChangeNotifier>();
		if (notifier is not null)
		{
			notifier.DataChanged += OnDataChanged;
		}
	}

	void OnDataChanged(string entityType, int entityId, DataChangeType changeType)
	{
		if (entityType == "Shot")
		{
			Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(() =>
			{
				LoadNextPage(reset: true);
				_filterVersion.Value++;
			});
		}
	}

	[Body]
	Comet.View body()
	{
		// Touch _filterVersion so body rebuilds when filters change
		var _ = _filterVersion.Value;

		if (_isLoading.Value)
		{
			LoadNextPage(reset: true);
			_isLoading.Value = false;
		}

		// Loading state
		if (_isLoading.Value)
		{
			var loadingStack = new VerticalStackLayout
			{
				BackgroundColor = Theme.Background,
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = LayoutOptions.Fill,
			};
			var indicator = new Microsoft.Maui.Controls.ActivityIndicator
			{
				IsRunning = true,
				Color = Theme.Primary,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};
			loadingStack.Add(indicator);
			return new MauiViewHost(loadingStack);
		}

		// Error state
		if (!string.IsNullOrEmpty(_errorMessage.Value))
		{
			var errorStack = new VerticalStackLayout
			{
				BackgroundColor = Theme.Background,
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = LayoutOptions.Fill,
			};
			errorStack.Add(FormHelpers.MakeEmptyState(Icons.Error, "Error Loading History", _errorMessage.Value));
			var retryBtn = new MauiButton
			{
				Text = "Retry",
				FontFamily = Theme.FontSemibold,
				BackgroundColor = Theme.Primary,
				TextColor = Colors.White,
				FontSize = 16,
				HeightRequest = Theme.ButtonHeight,
				CornerRadius = (int)Theme.RadiusPill,
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(0, Theme.SpacingM, 0, 0),
			};
			retryBtn.Clicked += (s, e) =>
			{
				_errorMessage.Value = "";
				_isLoading.Value = true;
			};
			errorStack.Add(retryBtn);
			return new MauiViewHost(errorStack);
		}

		// Unfiltered empty state
		if (_displayedShots.Count == 0 && !_filters.HasFilters)
		{
			var emptyStack = new VerticalStackLayout
			{
				BackgroundColor = Theme.Background,
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = LayoutOptions.Fill,
			};
			emptyStack.Add(FormHelpers.MakeEmptyState(Icons.Coffee, "No Shots Yet", "Start logging your espresso shots to see them here."));
			return new MauiViewHost(emptyStack);
		}

		var wrapper = new VerticalStackLayout { Spacing = 0, BackgroundColor = Theme.Background };

		// Spacer
		wrapper.Add(new MauiBoxView { HeightRequest = 20, BackgroundColor = Colors.Transparent });

		// Header row: shot count + filter button
		var headerRow = new HorizontalStackLayout
		{
			Spacing = Theme.SpacingS,
			Padding = new Thickness(Theme.SpacingM, Theme.SpacingS),
			VerticalOptions = LayoutOptions.Center,
		};

		var countText = _filters.HasFilters
			? $"{_filteredShotCount} of {_totalShotCount} shots"
			: $"{_totalShotCount} shots logged";
		headerRow.Add(new MauiLabel
		{
			Text = countText,
			FontFamily = Theme.FontSemibold,
			FontSize = 14,
			FontAttributes = MauiFontAttributes.Bold,
			TextColor = Theme.TextSecondary,
			VerticalTextAlignment = TextAlignment.Center,
			HorizontalOptions = LayoutOptions.Fill,
		});

		// Filter button with badge
		var filterBtn = new MauiButton
		{
			Text = _filters.HasFilters
				? $"{Icons.FilterList} {_filters.FilterCount}"
				: Icons.FilterList,
			FontFamily = _filters.HasFilters ? Theme.FontSemibold : Icons.FontFamily,
			FontSize = _filters.HasFilters ? 14 : 22,
			BackgroundColor = _filters.HasFilters ? Theme.Primary : Colors.Transparent,
			TextColor = _filters.HasFilters ? Colors.White : Theme.TextPrimary,
			BorderColor = _filters.HasFilters ? Theme.Primary : Theme.Outline,
			BorderWidth = 1,
			CornerRadius = (int)Theme.RadiusPill,
			HeightRequest = 36,
			MinimumWidthRequest = 36,
			Padding = _filters.HasFilters ? new Thickness(12, 0) : new Thickness(6, 0),
		};
		filterBtn.Clicked += OnFilterTapped;
		headerRow.Add(filterBtn);

		// Clear button (only when filters active)
		if (_filters.HasFilters)
		{
			var clearBtn = new MauiButton
			{
				Text = Icons.FilterListOff,
				FontFamily = Icons.FontFamily,
				FontSize = 20,
				BackgroundColor = Colors.Transparent,
				TextColor = Theme.Error,
				BorderColor = Theme.Error,
				BorderWidth = 1,
				CornerRadius = (int)Theme.RadiusPill,
				HeightRequest = 36,
				MinimumWidthRequest = 36,
				Padding = new Thickness(6, 0),
			};
			clearBtn.Clicked += (s, e) =>
			{
				_filters.Clear();
				LoadNextPage(reset: true);
				_filterVersion.Value++;
			};
			headerRow.Add(clearBtn);
		}

		wrapper.Add(headerRow);

		// Empty state for filtered results
		if (_displayedShots.Count == 0 && _filters.HasFilters)
		{
			wrapper.Add(FormHelpers.MakeEmptyState(Icons.FilterListOff, "No Matching Shots", "Try adjusting or clearing your filters"));
			var clearFiltersBtn = new MauiButton
			{
				Text = "Clear Filters",
				FontFamily = Theme.FontSemibold,
				BackgroundColor = Theme.Primary,
				TextColor = Colors.White,
				FontSize = 16,
				HeightRequest = Theme.ButtonHeight,
				CornerRadius = (int)Theme.RadiusPill,
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(0, Theme.SpacingM, 0, 0),
			};
			clearFiltersBtn.Clicked += (s, e) =>
			{
				_filters.Clear();
				LoadNextPage(reset: true);
				_filterVersion.Value++;
			};
			wrapper.Add(clearFiltersBtn);
			return new MauiViewHost(wrapper);
		}

		// Paginated CollectionView
		var collectionView = new MauiCollectionView
		{
			ItemsSource = _displayedShots,
			RemainingItemsThreshold = 5,
			BackgroundColor = Theme.Background,
			VerticalOptions = LayoutOptions.Fill,
			ItemTemplate = new DataTemplate(() =>
			{
				var container = new Microsoft.Maui.Controls.ContentView();
				container.BindingContextChanged += (s, e) =>
				{
					if (s is Microsoft.Maui.Controls.ContentView cv && cv.BindingContext is ShotRecord shot)
					{
						cv.Content = ShotRecordCardFactory.Create(shot, () =>
						{
							Shell.Current.GoToAsync($"shot-edit?id={shot.Id}");
						});
					}
				};
				return container;
			}),
		};

		collectionView.RemainingItemsThresholdReached += OnThresholdReached;

		wrapper.Add(collectionView);

		// Give the CollectionView room to fill remaining space
		collectionView.HeightRequest = 600;
		Microsoft.Maui.Controls.Grid.SetRow(collectionView, 1);

		return new MauiViewHost(wrapper);
	}

	async void OnFilterTapped(object? sender, EventArgs e)
	{
		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		// Build bean options: only beans that have at least one shot
		var allShots = store.GetAllShots();
		var beanNames = allShots
			.Where(s => s.BeanName != null)
			.Select(s => s.BeanName!)
			.Distinct()
			.ToList();
		var allBeans = store.GetAllBeans();
		var beanOptions = allBeans
			.Where(b => beanNames.Contains(b.Name))
			.Select(b => (b.Id, b.Name))
			.ToList();

		// Build people options: profiles that appear as MadeFor
		var madeForIds = allShots
			.Where(s => s.MadeForId.HasValue)
			.Select(s => s.MadeForId!.Value)
			.Distinct()
			.ToHashSet();
		var allProfiles = store.GetAllProfiles();
		var peopleOptions = allProfiles
			.Where(p => madeForIds.Contains(p.Id))
			.Select(p => (p.Id, p.Name))
			.ToList();

		var popup = new ShotFilterPopup(
			_filters,
			beanOptions,
			peopleOptions,
			onApply: applied =>
			{
				_filters.BeanIds = applied.BeanIds;
				_filters.MadeForIds = applied.MadeForIds;
				_filters.Ratings = applied.Ratings;
				LoadNextPage(reset: true);
				_filterVersion.Value++;
			},
			onClear: () =>
			{
				_filters.Clear();
				LoadNextPage(reset: true);
				_filterVersion.Value++;
			});

		await IPopupService.Current.PushAsync(popup);
	}

	void OnThresholdReached(object? sender, EventArgs e)
	{
		if (_hasMorePages)
		{
			LoadNextPage(reset: false);
		}
	}

	void LoadNextPage(bool reset)
	{
		var store = InMemoryDataStore.Instance;
		if (store == null) return;

		if (reset)
		{
			_currentPage = 0;
			_hasMorePages = true;
			_displayedShots.Clear();
		}

		var allShots = store.GetAllShots();
		_totalShotCount = allShots.Count;

		// Apply filters
		var filtered = ApplyFilters(allShots);
		_filteredShotCount = filtered.Count;

		var page = filtered.Skip(_currentPage * _pageSize).Take(_pageSize).ToList();

		foreach (var shot in page)
		{
			_displayedShots.Add(shot);
		}

		_currentPage++;
		_hasMorePages = page.Count == _pageSize;
	}

	List<ShotRecord> ApplyFilters(List<ShotRecord> shots)
	{
		if (!_filters.HasFilters)
			return shots;

		var store = InMemoryDataStore.Instance;

		return shots.Where(s =>
		{
			// Bean filter: match by looking up the bag's bean ID
			if (_filters.BeanIds.Count > 0)
			{
				var bag = store?.GetBag(s.BagId);
				if (bag == null || !_filters.BeanIds.Contains(bag.BeanId))
					return false;
			}

			// Made-for filter
			if (_filters.MadeForIds.Count > 0)
			{
				if (!s.MadeForId.HasValue || !_filters.MadeForIds.Contains(s.MadeForId.Value))
					return false;
			}

			// Rating filter
			if (_filters.Ratings.Count > 0)
			{
				if (!s.Rating.HasValue || !_filters.Ratings.Contains(s.Rating.Value))
					return false;
			}

			return true;
		}).ToList();
	}
}
