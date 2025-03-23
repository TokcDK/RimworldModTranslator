using System;
using RimworldModTranslator.ViewModels;
using RimworldModTranslator.Views;

namespace RimworldModTranslator;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
        
    /// <summary>
    /// Application Entry for RimworldModTranslator
    /// </summary>
    public App()
    {
        var view = new MainView
        {
            DataContext = Activator.CreateInstance<MainViewModel>()
        };
            
        view.Show();
    }
        
}