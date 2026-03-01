using System.Drawing;

namespace CursorCompanionFinish
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // УДАЛИТЬ ВЕСЬ МЕТОД DISPOSE! ОН УЖЕ ЕСТЬ В MainForm.cs

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Icon = new Icon("CCicon.ico");
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.ResumeLayout(false);

        }
    }
}