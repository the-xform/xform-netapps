namespace MySampleWinFormsApp
{
	partial class Form1
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			label1 = new Label();
			textBox1 = new TextBox();
			SuspendLayout();
			// 
			// label1
			// 
			label1.Font = new Font("Segoe UI", 24F);
			label1.Location = new Point(22, 44);
			label1.Name = "label1";
			label1.Size = new Size(749, 59);
			label1.TabIndex = 0;
			label1.Text = "label1";
			label1.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// textBox1
			// 
			textBox1.Font = new Font("Courier New", 9F);
			textBox1.Location = new Point(22, 142);
			textBox1.Multiline = true;
			textBox1.Name = "textBox1";
			textBox1.ReadOnly = true;
			textBox1.ScrollBars = ScrollBars.Vertical;
			textBox1.Size = new Size(749, 259);
			textBox1.TabIndex = 1;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(textBox1);
			Controls.Add(label1);
			Name = "Form1";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "Form1";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Label label1;
		private TextBox textBox1;
	}
}
