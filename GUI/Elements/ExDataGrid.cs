using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace GUI.Elements
{
    class ExDataGrid : DataGrid
    {
         public ExDataGrid()
        {
            this.SelectionChanged += ExDataGrid_SelectionChanged;
        }

        void ExDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SelectedItemsList = SelectedItems;

                SingleSelect = SelectedItems.Count == 1;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
        }
        #region SelectedItemsList

        public IList SelectedItemsList
        {
            get { return (IList)GetValue(SelectedItemsListProperty); }
            set { SetValue(SelectedItemsListProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsListProperty =
                DependencyProperty.Register(
                    "SelectedItemsList", 
                    typeof(IList), 
                    typeof(ExDataGrid), new PropertyMetadata(null));


        public static void OnSelectedItemsListChangeCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var value = e.NewValue;
        }

        public bool SingleSelect
        {
            get { return (bool)GetValue(SingleSelectProperty); }
            set { SetValue(SingleSelectProperty, value); }
        }
        // Using a DependencyProperty as the backing store for SingleSelect.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SingleSelectProperty =
            DependencyProperty.Register("SingleSelect", typeof(bool), typeof(ExDataGrid), new PropertyMetadata(null));
        #endregion
    }
}
