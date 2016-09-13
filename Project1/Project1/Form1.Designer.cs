namespace Project1
{
    partial class Form1
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
            this.input = new System.Windows.Forms.TextBox();
            this.k_hole = new System.Windows.Forms.TextBox();
            this.output = new System.Windows.Forms.TextBox();
            this.button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // input
            // 
            this.input.Location = new System.Drawing.Point(26, 23);
            this.input.Name = "input";
            this.input.Size = new System.Drawing.Size(100, 20);
            this.input.TabIndex = 0;
            this.input.Text = "To check";
            // 
            // k_hole
            // 
            this.k_hole.Location = new System.Drawing.Point(26, 108);
            this.k_hole.Name = "k_hole";
            this.k_hole.Size = new System.Drawing.Size(100, 20);
            this.k_hole.TabIndex = 1;
            this.k_hole.Text = "Value of k";
            // 
            // output
            // 
            this.output.Location = new System.Drawing.Point(150, 52);
            this.output.Multiline = true;
            this.output.Name = "output";
            this.output.Size = new System.Drawing.Size(100, 76);
            this.output.TabIndex = 2;
            this.output.Text = "Result";
            // 
            // button
            // 
            this.button.Location = new System.Drawing.Point(150, 23);
            this.button.Name = "button";
            this.button.Size = new System.Drawing.Size(100, 23);
            this.button.TabIndex = 3;
            this.button.Text = "Solve";
            this.button.UseVisualStyleBackColor = true;
            this.button.Click += new System.EventHandler(this.button_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 146);
            this.Controls.Add(this.button);
            this.Controls.Add(this.output);
            this.Controls.Add(this.k_hole);
            this.Controls.Add(this.input);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox input;
        private System.Windows.Forms.TextBox k_hole;
        private System.Windows.Forms.TextBox output;
        private System.Windows.Forms.Button button;
    }
}

