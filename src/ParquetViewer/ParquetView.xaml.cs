﻿using Parquet.Data;
using ParquetViewer.Model;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ParquetViewer
{
   public sealed partial class ParquetView : UserControl
   {
      private ParquetIncrementalList _rowsList;

      public ParquetView()
      {
         this.InitializeComponent();
      }

      public async Task DisplayAsync(StorageFile file)
      {
         SfGrid.Columns.Clear();
         _rowsList = new ParquetIncrementalList(file, LoadRowsAsync);
         await _rowsList.InitialiseAsync();

         if (_rowsList.Schema == null)
         {
            return;
         }

         for (int i = 0; i < _rowsList.Schema.Length; i++)
         {
            SfGrid.Columns.Add(CreateSfColumn(_rowsList.Schema[i], i));
         }

         SfGrid.ItemsSource = _rowsList;
      }

      private async Task<IList<TableRowView>> LoadRowsAsync(CancellationToken token, uint count, int baseIndex)
      {
         var result = await _rowsList.LoadRowsAsync(token, count, baseIndex);

         ulong memSize = ProcessDiagnosticInfo.GetForCurrentProcess().MemoryUsage.GetReport().WorkingSetSizeInBytes;

         StatusText.Text = string.Format("total: {0:N0} | cached: {1:N0} | memory used: {2}",
            _rowsList.MaxItemCount, _rowsList.CachedRowsCount, ((long)memSize).ToFileSizeUiString());

         return result;
      }

      private GridColumn CreateSfColumn(Field field, int i)
      {
         GridColumn result;

         DataType t = field.SchemaType == SchemaType.Data
            ? ((DataField)field).DataType
            : DataType.String;

         if (t == DataType.Int32 ||
            t == DataType.Float ||
            t == DataType.Double ||
            t == DataType.Decimal)
         {
            result = new GridNumericColumn();
         }
         else if (t == DataType.DateTimeOffset)
         {
            result = new GridDateTimeColumn();
         }
         else if (t == DataType.Boolean)
         {
            result = new GridCheckBoxColumn();
         }
         else
         {
            result = new GridTextColumn() { TextWrapping = TextWrapping.NoWrap };
         }

         result.MappingName = $"[{i}]";
         result.HeaderText = field.Name;
         result.AllowFiltering = false;
         result.AllowFocus = true;
         result.AllowResizing = true;
         result.AllowSorting = true;
         result.FilterBehavior = FilterBehavior.StronglyTyped;
         result.AllowEditing = true;

         return result;
      }

   }
}
