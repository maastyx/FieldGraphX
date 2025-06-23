using System;
using System.Windows.Forms;

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

        private void InitializeComponent()
        {
            this.cmbEntities = new System.Windows.Forms.ComboBox();
            this.cmbFields = new System.Windows.Forms.ComboBox();
            this.lblEntity = new System.Windows.Forms.Label();
            this.lblField = new System.Windows.Forms.Label();
            this.btnAnalyze = new System.Windows.Forms.Button();
            this.tvResults = new System.Windows.Forms.TreeView();
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.flpResults = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // cmbEntities
            // 
            this.cmbEntities.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbEntities.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbEntities.FormattingEnabled = true;
            this.cmbEntities.Location = new System.Drawing.Point(120, 30);
            this.cmbEntities.Name = "cmbEntities";
            this.cmbEntities.Size = new System.Drawing.Size(200, 21);
            this.cmbEntities.TabIndex = 0;
            this.cmbEntities.SelectedIndexChanged += new System.EventHandler(this.cmbEntities_SelectedIndexChanged);
            // 
            // cmbFields
            // 
            this.cmbFields.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbFields.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbFields.FormattingEnabled = true;
            this.cmbFields.Location = new System.Drawing.Point(120, 70);
            this.cmbFields.Name = "cmbFields";
            this.cmbFields.Size = new System.Drawing.Size(200, 21);
            this.cmbFields.TabIndex = 1;
            // 
            // lblEntity
            // 
            this.lblEntity.AutoSize = true;
            this.lblEntity.Location = new System.Drawing.Point(20, 30);
            this.lblEntity.Name = "lblEntity";
            this.lblEntity.Size = new System.Drawing.Size(94, 13);
            this.lblEntity.TabIndex = 2;
            this.lblEntity.Text = "Entität auswählen:";
            // 
            // lblField
            // 
            this.lblField.AutoSize = true;
            this.lblField.Location = new System.Drawing.Point(20, 70);
            this.lblField.Name = "lblField";
            this.lblField.Size = new System.Drawing.Size(84, 13);
            this.lblField.TabIndex = 3;
            this.lblField.Text = "Feld auswählen:";
            // 
            // btnAnalyze
            // 
            this.btnAnalyze.Location = new System.Drawing.Point(386, 70);
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.Size = new System.Drawing.Size(75, 23);
            this.btnAnalyze.TabIndex = 4;
            this.btnAnalyze.Text = "Analysieren";
            this.btnAnalyze.UseVisualStyleBackColor = true;
            this.btnAnalyze.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // tvResults
            // 
            this.tvResults.Location = new System.Drawing.Point(20, 113);
            this.tvResults.Name = "tvResults";
            this.tvResults.Size = new System.Drawing.Size(702, 219);
            this.tvResults.TabIndex = 5;
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Size = new System.Drawing.Size(100, 25);
            this.toolStripMenu.TabIndex = 0;
            // 
            // tsbClose
            // 
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Size = new System.Drawing.Size(23, 23);
            // 
            // tssSeparator1
            // 
            this.tssSeparator1.Name = "tssSeparator1";
            this.tssSeparator1.Size = new System.Drawing.Size(6, 6);
            // 
            // flpResults
            // 
            this.flpResults.AutoScroll = true;
            this.flpResults.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpResults.Location = new System.Drawing.Point(20, 338);
            this.flpResults.Name = "flpResults";
            this.flpResults.Size = new System.Drawing.Size(1329, 367);
            this.flpResults.TabIndex = 6;
            // 
            // FieldGraphXControl
            // 
            this.Controls.Add(this.cmbEntities);
            this.Controls.Add(this.cmbFields);
            this.Controls.Add(this.lblEntity);
            this.Controls.Add(this.lblField);
            this.Controls.Add(this.btnAnalyze);
            this.Controls.Add(this.tvResults);
            this.Controls.Add(this.flpResults);
            this.Name = "FieldGraphXControl";
            this.Size = new System.Drawing.Size(1383, 708);
            this.Load += new System.EventHandler(this.FieldGraphXControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }




        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;

        private System.Windows.Forms.Label lblEntity;
        private System.Windows.Forms.Label lblField;
        private System.Windows.Forms.Button btnAnalyze;
        private System.Windows.Forms.TreeView tvResults;
        private ComboBox cmbEntities;
        private ComboBox cmbFields;
        private FlowLayoutPanel flpResults;
    }
}
