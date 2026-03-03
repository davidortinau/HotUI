using System;
using System.Collections.Generic;

namespace Comet
{
	/// <summary>
	/// Builder for fluent visual state configuration in MVU patterns.
	/// Usage: myButton.VisualStates(states =&gt; states
	///     .State("Normal", s =&gt; s.Set("BackgroundColor", Colors.Blue))
	///     .State("Pressed", s =&gt; s.Set("BackgroundColor", Colors.DarkBlue))
	///     .State("Disabled", s =&gt; s.Set("BackgroundColor", Colors.Gray))
	/// )
	/// </summary>
	public class VisualStateGroupBuilder
	{
		readonly List<VisualStateGroup> _groups = new();
		VisualStateGroup _currentGroup;

		public VisualStateGroupBuilder(string groupName = VisualStateManager.CommonStates)
		{
			_currentGroup = new VisualStateGroup { Name = groupName };
			_groups.Add(_currentGroup);
		}

		/// <summary>
		/// Starts a new visual state group.
		/// </summary>
		public VisualStateGroupBuilder Group(string name)
		{
			_currentGroup = new VisualStateGroup { Name = name };
			_groups.Add(_currentGroup);
			return this;
		}

		/// <summary>
		/// Adds a visual state with explicit setters.
		/// </summary>
		public VisualStateGroupBuilder State(string name, params Setter[] setters)
		{
			var state = new VisualState { Name = name };
			foreach (var setter in setters)
				state.Setters.Add(setter);
			_currentGroup.States.Add(state);
			return this;
		}

		/// <summary>
		/// Adds a visual state configured via a fluent setter builder.
		/// </summary>
		public VisualStateGroupBuilder State(string name, Action<SetterBuilder> configure)
		{
			var builder = new SetterBuilder();
			configure(builder);
			var state = new VisualState { Name = name };
			foreach (var setter in builder.Build())
				state.Setters.Add(setter);
			_currentGroup.States.Add(state);
			return this;
		}

		internal List<VisualStateGroup> Build() => _groups;
	}

	/// <summary>
	/// Fluent builder for creating setters within a visual state.
	/// </summary>
	public class SetterBuilder
	{
		readonly List<Setter> _setters = new();

		/// <summary>
		/// Sets a property to the specified value.
		/// </summary>
		public SetterBuilder Set(string property, object value)
		{
			_setters.Add(new Setter { Property = property, Value = value });
			return this;
		}

		internal List<Setter> Build() => _setters;
	}

	/// <summary>
	/// Extension methods for fluent visual state configuration.
	/// </summary>
	public static class VisualStateExtensions
	{
		/// <summary>
		/// Configures visual states using a fluent builder pattern.
		/// </summary>
		public static T VisualStates<T>(this T view, Action<VisualStateGroupBuilder> configure) where T : View
		{
			var builder = new VisualStateGroupBuilder();
			configure(builder);
			VisualStateManager.SetVisualStateGroups(view, builder.Build());
			return view;
		}

		/// <summary>
		/// Configures visual states in a named group using a fluent builder pattern.
		/// </summary>
		public static T VisualStates<T>(this T view, string groupName, Action<VisualStateGroupBuilder> configure) where T : View
		{
			var builder = new VisualStateGroupBuilder(groupName);
			configure(builder);
			VisualStateManager.SetVisualStateGroups(view, builder.Build());
			return view;
		}
	}
}
