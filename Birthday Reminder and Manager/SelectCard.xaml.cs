using BirthdayReminder.Resources;
using BirthdayReminderCore.Models;
using BirthdayReminderCore.Utilities;
using Microsoft.Phone.Controls;
using System;
using System.Windows;

namespace BirthdayReminder
{
    public partial class SelectCard : PhoneApplicationPage
    {
        public SelectCard()
        {
            InitializeComponent();
            DataContext = BirthdayUtility.GetBirthdayCards();
        }

        private void CardImageTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                var selector = sender as LongListSelector;
                if (selector != null)
                {
                    var cardEntity = selector.SelectedItem as CardEntity;
                    if (cardEntity != null)
                        App.UserPreferences.BirthdayWish.CustomPicUrl = cardEntity.Url;
                }
                SettingsUtility.UpdateUserSettings(App.UserPreferences);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppResources.ErrSelecting, MessageBoxButton.OK);
            }
            finally
            {
                NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
            }
        }
    }
}