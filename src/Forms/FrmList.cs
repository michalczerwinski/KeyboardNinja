
using KeyboardNinja.Helpers;
using SharpHook.Native;
using System.Text;

namespace KeyboardNinja;

public partial class FrmList : Form
{
	public FrmList()
	{
		InitializeComponent();
	}



	private void FrmList_Shown(object sender, EventArgs e)
	{
		TopMost = true;
		TopLevel = true;
		BringToFront();
		Activate();
	}

	private void FrmList_Deactivate(object sender, EventArgs e)
	{
		Close();
	}
}
