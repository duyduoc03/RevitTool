using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using RevitTool.Models;
using System;
using System.Collections.Generic;

namespace RevitTool.ViewModels
{
    /// <summary>
    ///     Dùng chung cho mọi tab loại "danh sách + refresh" (Wall, Door, Furniture, Rebar, ...).
    ///     Truyền vào hàm lấy dữ liệu tương ứng của Service - không cần viết riêng
    ///     List/Count/RefreshCommand cho từng loại element.
    /// </summary>
    public partial class ElementTabViewModel<TModel> : ObservableObject where TModel : IElementModel
    {
        private readonly Func<Document, List<TModel>> fetcher;

        public ElementTabViewModel(Func<Document, List<TModel>> fetcher)
        {
            this.fetcher = fetcher;
        }

        [ObservableProperty]
        private List<TModel> items = new();

        [ObservableProperty]
        private int count;

        [RelayCommand]
        private void Refresh()
        {
            Document doc = Context.ActiveDocument;

            if (doc == null)
            {
                return;
            }

            List<TModel> list = fetcher(doc);

            Items = list;
            Count = list.Count;
        }
    }
}