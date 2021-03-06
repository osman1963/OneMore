﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Dialogs;
	using System;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class SplitDialog : LocalizableForm
	{
		public SplitDialog()
		{
			InitializeComponent();

			TagSymbol = 0;

			if (NeedsLocalizing())
			{
				Text = Resx.SplitDialog_Text;

				Localize(new string[]
				{
					"splitByGroup",
					"byHeading1Box",
					"byHeading1Label",
					"byLinksBox",
					"byLinksLabel",
					"filterGroup",
					"taggedBox",
					"tagLabel",
					"okButton",
					"cancelButton"
				});
			}
		}


		protected override void OnShown(EventArgs e)
		{
			Location = new System.Drawing.Point(Location.X, Location.Y - (Height / 2));
			UIHelper.SetForegroundWindow(this);
		}


		public bool SplitByHeading => byHeading1Box.Checked;


		public bool SplitByLink => byLinksBox.Checked;


		public bool Tagged => taggedBox.Checked;


		public int TagSymbol { get; private set; }


		private void ToggleTagged(object sender, EventArgs e)
		{
			tagButton.Enabled = taggedBox.Checked;
		}

		private void SetTag(object sender, EventArgs e)
		{
			var location = PointToScreen(tagButton.Location);

			using (var dialog = new TagPickerDialog(
				location.X + tagButton.Bounds.Location.X - tagButton.Width,
				location.Y + tagButton.Bounds.Location.Y))
			{
				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					var glyph = dialog.GetGlyph();
					if (glyph != null)
					{
						tagButton.Text = null;
						tagButton.Image = glyph;
					}
					else
					{
						tagButton.Text = "?";
					}

					TagSymbol = dialog.Symbol;
				}
			}
		}
	}
}
