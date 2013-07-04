using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace WPFUserControlLibrary
{
    public class DropDownButton : ToggleButton
    {
        // *** Dependency Properties ***

        public static readonly DependencyProperty DropDownProperty =
          DependencyProperty.Register("DropDown",
                                      typeof(ContextMenu),
                                      typeof(DropDownButton),
                                      new UIPropertyMetadata(null));

        // *** Constructors *** 

        public DropDownButton()
        {
            // Bind the ToogleButton.IsChecked property to the drop-down's IsOpen property 

            Binding binding = new Binding("DropDown.IsOpen");
            binding.Source = this;
            this.SetBinding(IsCheckedProperty, binding);
        }

        // *** Properties *** 

        public ContextMenu DropDown
        {
            get { return (ContextMenu)this.GetValue(DropDownProperty); }
            set { this.SetValue(DropDownProperty, value); }
        }

        // *** Overridden Methods *** 

        protected override void OnClick()
        {
            if (this.DropDown != null)
            {
                // If there is a drop-down assigned to this button, then position and display it 

                this.DropDown.PlacementTarget = this;
                this.DropDown.Placement = PlacementMode.Bottom;

                this.DropDown.IsOpen = true;
            }
        }
    }
}
