﻿using System;
namespace HotUI {
	public class WebView : Control {
		string html;
		public string Html {
			get => html;
			set => this.SetValue (State, ref html, value);
		}

		string source;
		public string Source {
			get => source;
			set => this.SetValue (State, ref source, value);
		}

		
	}
}
