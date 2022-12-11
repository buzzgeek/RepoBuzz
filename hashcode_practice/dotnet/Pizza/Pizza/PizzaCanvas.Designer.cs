namespace Pizza
{
    partial class PizzaCanvas
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // PizzaCanvas
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoScroll = true;
            this.Name = "PizzaCanvas";
            this.Size = new System.Drawing.Size(643, 499);
            this.Scroll += new System.Windows.Forms.ScrollEventHandler(this.PizzaCanvas_Scroll);
            this.SizeChanged += new System.EventHandler(this.PizzaCanvas_SizeChanged);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
