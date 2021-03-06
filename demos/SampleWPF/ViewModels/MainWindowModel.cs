using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurVersionControl.SampleWPF.ViewModels
{
    internal class MainWindowModel
    {
        public BooksCRUDPresenter CrudPresenter { get; set; } = new BooksCRUDPresenter();
    }
}
