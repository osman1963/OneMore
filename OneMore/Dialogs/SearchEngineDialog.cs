﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant
#pragma warning disable IDE1006 // Words must begin with upper case

namespace River.OneMoreAddIn.Dialogs
{
	using River.OneMoreAddIn.Helpers.Settings;
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Linq;
	using System.Net;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class SearchEngineDialog : LocalizableForm
	{
		private readonly BindingList<SearchEngine> engines;


		public SearchEngineDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.SearchEngineDialog_Text;

				Localize(new string[]
				{
					"introLabel",
					"upButton",
					"downButton",
					"refreshButton",
					"deleteLabel",
					"deleteButton",
					"okButton",
					"cancelButton"
				});

				iconColumn.HeaderText = Resx.SearchEngineDialog_iconColumn_HeaderText;
				nameColumn.HeaderText = Resx.SearchEngineDialog_nameColumn_HeaderText;
				urlColumn.HeaderText = Resx.SearchEngineDialog_urlColumn_HeaderText;
			}

			// prevent VS designer from overriding
			toolStrip.ImageScalingSize = new Size(16, 16);

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = "Image";
			gridView.Columns[1].DataPropertyName = "Name";
			gridView.Columns[2].DataPropertyName = "Uri";

			engines = new BindingList<SearchEngine>(new SearchEngineProvider().Load());

			gridView.DataSource = engines;

			RefreshImages();
		}

		private void RefreshImages()
		{
			foreach (var engine in engines)
			{
				if (engine.Image == null)
				{
					RefreshImage(engine);
				}
			}
		}


		private void RefreshImage(SearchEngine engine)
		{
			ServicePointManager.SecurityProtocol =
				SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

			try
			{
				var uri = new Uri(engine.Uri);
				var url = string.Format("https://{0}/favicon.ico", uri.Host);

				var request = WebRequest.Create(url);
				using (var response = request.GetResponse())
				{
					using (var stream = response.GetResponseStream())
					{
						engine.Image = new Bitmap(Image.FromStream(stream), 16, 16);
					}
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"Error getting favicon for {engine.Uri}", exc);
			}
		}


		protected override void OnShown(EventArgs e)
		{
			Location = new Point(Location.X, Location.Y - (Height / 2));
			base.OnShown(e);
		}


		private void gridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == 2)
			{
				if (engines[e.RowIndex] is SearchEngine engine)
				{
					RefreshImage(engine);
					gridView.Refresh();
				}
			}
		}


		private void gridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
		{
			// overrides the "red x" icon in the new-item row
			gridView.Rows[gridView.Rows.Count - 1].Cells[0].Value =
				((DataGridViewImageColumn)gridView.Columns[0]).Image;

			//if (!loaded)
			//{
			//	gridView.Rows[gridView.Rows.Count - 1].Cells[1].Selected = true;
			//	loaded = true;
			//}
		}


		private void refreshButton_Click(object sender, EventArgs e)
		{
			int rowIndex = -1;
			if (gridView.SelectedCells.Count > 0)
			{
				rowIndex = gridView.SelectedCells[0].RowIndex;
			}
			else if (gridView.SelectedRows.Count > 0)
			{
				rowIndex = gridView.SelectedRows[0].Index;
			}

			if (rowIndex >= 0)
			{
				if (engines[rowIndex] is SearchEngine engine)
				{
					RefreshImage(engine);
				}
			}
		}


		private void gridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
		{
			var result = MessageBox.Show(
				string.Format(Resx.SearchEngineDialog_DeleteMessage, engines[e.Row.Index].Name),
				"OneMore",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2,
				MessageBoxOptions.DefaultDesktopOnly);

			if (result != DialogResult.Yes)
			{
				e.Cancel = true;
			}
		}


		private void deleteButton_Click(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count > 0)
			{
				int colIndex = gridView.SelectedCells[0].ColumnIndex;
				int rowIndex = gridView.SelectedCells[0].RowIndex;
				if (rowIndex < engines.Count)
				{
					var result = MessageBox.Show(
						string.Format(Resx.SearchEngineDialog_DeleteMessage, engines[rowIndex].Name),
						"OneMore",
						MessageBoxButtons.YesNo, MessageBoxIcon.Question,
						MessageBoxDefaultButton.Button2,
						MessageBoxOptions.DefaultDesktopOnly);

					if (result == DialogResult.Yes)
					{
						engines.RemoveAt(rowIndex);

						if (rowIndex > 0)
						{
							rowIndex--;
						}

						gridView.Rows[rowIndex].Cells[colIndex].Selected = true;
					}
				}
			}
		}


		private void upButton_Click(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count > 0)
			{
				int colIndex = gridView.SelectedCells[0].ColumnIndex;
				int rowIndex = gridView.SelectedCells[0].RowIndex;
				if (rowIndex > 0 && rowIndex < engines.Count)
				{
					var item = engines[rowIndex];
					engines.RemoveAt(rowIndex);
					engines.Insert(rowIndex - 1, item);

					gridView.Rows[rowIndex - 1].Cells[colIndex].Selected = true;
				}
			}
		}


		private void downButton_Click(object sender, EventArgs e)
		{
			if (gridView.SelectedCells.Count > 0)
			{
				int colIndex = gridView.SelectedCells[0].ColumnIndex;
				int rowIndex = gridView.SelectedCells[0].RowIndex;
				if (rowIndex < engines.Count - 1)
				{
					var item = engines[rowIndex];
					engines.RemoveAt(rowIndex);
					engines.Insert(rowIndex + 1, item);

					gridView.Rows[rowIndex + 1].Cells[colIndex].Selected = true;
				}
			}
		}


		private void okButton_Click(object sender, EventArgs e)
		{
			new SearchEngineProvider().Save(engines.ToList());
		}
	}
}