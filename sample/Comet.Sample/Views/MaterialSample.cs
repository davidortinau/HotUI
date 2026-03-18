using Comet.Styles.Material;
using System;
using System.Collections.Generic;
using System.Text;
using static Comet.CometControls;

namespace Comet.Samples
{
	public class MaterialSample : Component
	{
				public override View Render() => VStack(
			HStack(
				Button("Contained Button").StyleAsContained(),
				Button("Outlined Button").StyleAsOutlined(),
				Button("Text Button").StyleAsText()
			)
		);

	}
}
