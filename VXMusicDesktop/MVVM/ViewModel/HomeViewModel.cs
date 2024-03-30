using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXMusicDesktop.MVVM.ViewModel
{
    public class HomeViewModel
    {
        // Shared ViewModel for sharing concurrency values between certain Views.
        public SharedViewModel SharedViewModel { get; }
        
        public HomeViewModel(SharedViewModel sharedViewModel)
        {
            SharedViewModel = sharedViewModel;
        }
    }
}
