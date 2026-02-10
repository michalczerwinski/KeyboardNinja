using KeyboardNinja.Helpers;
using SharpHook.Native;

namespace KeyboardNinja.Mappings.Navigation;

public record class ShowHelp() : MappingRule("Miscellaneous", "Show help", KeyCode.VcF, KeyCode.VcSlash)
{
	public override Task ExecutePressAsync()
	{
		return Task.Run(() => FormHelper.ToggleForm<FrmHelp>());
	}
}


//public record class ShowHelp() : MappingRule("Miscellaneous", "Test picker", KeyCode.VcF, KeyCode.VcP)
//{
//	private HWND currentWindow;

//    public override Task ExecutePressAsync()
//    {
//		currentWindow = PInvoke.GetForegroundWindow();

//		return Task.Run(() => FormHelper.ToggleForm<FrmList>(Initialize));
//    }

//	private void Initialize(FrmList form)
//	{
//		form.ListBox.Items.Clear();

//		foreach (var desktop in FormHelper.GetWindowDesktops())
//		{
//			var item = new ListViewItem(desktop.DesktopName)
//			{			
//				Text = desktop.DesktopName,
//				Tag = desktop.DesktopId
//			};
//			form.ListBox.Items.Add(item);
//		}

//		form.ListBox.SelectedIndexChanged += (s, e) =>
//		{
//			var selectedItem = form.ListBox.SelectedItems[0];
//			var desktopId = (Guid)((ListViewItem)selectedItem).Tag;
//		};
//	}
//}
