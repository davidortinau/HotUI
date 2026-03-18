using System;
using System.Collections.Generic;
using static Comet.CometControls;

/*

struct ContentView : View {
    @State private var password: String = ""

    var body: some View {
        VStack {
            SecureField("Enter a password", text: $password)
            Text("You entered: \(password)")
        }
    }
}

*/
namespace Comet.Samples
{
	public class SecureFieldSample3 : Component
	{
		readonly State<string> password = new State<string>("");

				public override View Render() => VStack(
			SecureField(password, "Enter a password"),
			Text(() => password.Value)
		).FillHorizontal();
	}
}
