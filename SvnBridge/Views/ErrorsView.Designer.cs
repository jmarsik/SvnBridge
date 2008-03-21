namespace SvnBridge.Views
{
	partial class ErrorsView
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lbErrors = new System.Windows.Forms.ListView();
			this.ErrorHeader = new System.Windows.Forms.ColumnHeader();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.txtErrorDetails = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// lbErrors
			// 
			this.lbErrors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ErrorHeader});
			this.lbErrors.Dock = System.Windows.Forms.DockStyle.Top;
			this.lbErrors.FullRowSelect = true;
			this.lbErrors.Location = new System.Drawing.Point(0, 0);
			this.lbErrors.MultiSelect = false;
			this.lbErrors.Name = "lbErrors";
			this.lbErrors.Size = new System.Drawing.Size(567, 97);
			this.lbErrors.TabIndex = 0;
			this.lbErrors.UseCompatibleStateImageBehavior = false;
			this.lbErrors.View = System.Windows.Forms.View.Details;
			this.lbErrors.SelectedIndexChanged += new System.EventHandler(this.lbErrors_SelectedIndexChanged);
			// 
			// ErrorHeader
			// 
			this.ErrorHeader.Text = "Error";
			this.ErrorHeader.Width = 550;
			// 
			// textBox1
			// 
			this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.textBox1.Enabled = false;
			this.textBox1.Location = new System.Drawing.Point(0, 97);
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size(567, 20);
			this.textBox1.TabIndex = 1;
			this.textBox1.Text = "Details:";
			// 
			// txtErrorDetails
			// 
			this.txtErrorDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtErrorDetails.Location = new System.Drawing.Point(0, 117);
			this.txtErrorDetails.Multiline = true;
			this.txtErrorDetails.Name = "txtErrorDetails";
			this.txtErrorDetails.ReadOnly = true;
			this.txtErrorDetails.Size = new System.Drawing.Size(567, 165);
			this.txtErrorDetails.TabIndex = 2;
			// 
			// ErrorsView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(567, 282);
			this.Controls.Add(this.txtErrorDetails);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.lbErrors);
			this.Name = "ErrorsView";
			this.Text = "Errors";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView lbErrors;
		private System.Windows.Forms.ColumnHeader ErrorHeader;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.TextBox txtErrorDetails;
	}
}