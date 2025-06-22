namespace FieldGraphX
{
    partial class FieldGraphXControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FieldGraphXControl));
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbSample = new System.Windows.Forms.ToolStripButton();

            this.lblEntity = new System.Windows.Forms.Label();
            this.txtEntity = new System.Windows.Forms.TextBox();
            this.lblField = new System.Windows.Forms.Label();
            this.txtField = new System.Windows.Forms.TextBox();
            this.btnAnalyze = new System.Windows.Forms.Button();
            this.tvResults = new System.Windows.Forms.TreeView();

            this.toolStripMenu.SuspendLayout();
            this.SuspendLayout();

            // toolStripMenu
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.tsbClose,
                this.tssSeparator1,
                this.tsbSample});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripMenu.Size = new System.Drawing.Size(839, 31);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "toolStrip1";

            // tsbClose
            this.tsbClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Size = new System.Drawing.Size(100, 28);
            this.tsbClose.Text = "Close this tool";
            this.tsbClose.Click += new System.EventHandler(this.tsbClose_Click);

            // tssSeparator1
            this.tssSeparator1.Name = "tssSeparator1";
            this.tssSeparator1.Size = new System.Drawing.Size(6, 31);

            // tsbSample
            this.tsbSample.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbSample.Name = "tsbSample";
            this.tsbSample.Size = new System.Drawing.Size(100, 28);
            this.tsbSample.Text = "Try me";
            this.tsbSample.Click += new System.EventHandler(this.tsbSample_Click);

            // lblEntity
            this.lblEntity.AutoSize = true;
            this.lblEntity.Location = new System.Drawing.Point(20, 50);
            this.lblEntity.Name = "lblEntity";
            this.lblEntity.Size = new System.Drawing.Size(54, 20);
            this.lblEntity.Text = "Entity:";

            // txtEntity
            this.txtEntity.Location = new System.Drawing.Point(100, 47);
            this.txtEntity.Name = "txtEntity";
            this.txtEntity.Size = new System.Drawing.Size(200, 26);

            // lblField
            this.lblField.AutoSize = true;
            this.lblField.Location = new System.Drawing.Point(320, 50);
            this.lblField.Name = "lblField";
            this.lblField.Size = new System.Drawing.Size(46, 20);
            this.lblField.Text = "Field:";

            // txtField
            this.txtField.Location = new System.Drawing.Point(370, 47);
            this.txtField.Name = "txtField";
            this.txtField.Size = new System.Drawing.Size(200, 26);

            // btnAnalyze
            this.btnAnalyze.Location = new System.Drawing.Point(590, 45);
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.Size = new System.Drawing.Size(120, 30);
            this.btnAnalyze.Text = "Analysieren";
            this.btnAnalyze.Click += new System.EventHandler(this.btnSearch_Click);

            // tvResults
            this.tvResults.Location = new System.Drawing.Point(20, 90);
            this.tvResults.Name = "tvResults";
            this.tvResults.Size = new System.Drawing.Size(790, 350);

            // FieldGraphXControl
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStripMenu);
            this.Controls.Add(this.lblEntity);
            this.Controls.Add(this.txtEntity);
            this.Controls.Add(this.lblField);
            this.Controls.Add(this.txtField);
            this.Controls.Add(this.btnAnalyze);
            this.Controls.Add(this.tvResults);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FieldGraphXControl";
            this.Size = new System.Drawing.Size(839, 462);
            this.Load += new System.EventHandler(this.MyPluginControl_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private System.Windows.Forms.ToolStripButton tsbSample;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;

        private System.Windows.Forms.Label lblEntity;
        private System.Windows.Forms.TextBox txtEntity;
        private System.Windows.Forms.Label lblField;
        private System.Windows.Forms.TextBox txtField;
        private System.Windows.Forms.Button btnAnalyze;
        private System.Windows.Forms.TreeView tvResults;
    }
}
