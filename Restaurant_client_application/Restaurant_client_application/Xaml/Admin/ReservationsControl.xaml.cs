﻿using pl.edu.wat.wcy.pz.restaurant_client_application.Controllers;
using pl.edu.wat.wcy.pz.restaurant_client_application.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace pl.edu.wat.wcy.pz.restaurant_client_application.Xaml.Admin
{
    /// <summary>
    /// Interaction logic for ReservationsControl.xaml
    /// </summary>
    public partial class ReservationsControl : UserControl
    {
        private List<Reservations> reservationLists = new List<Reservations>();
        public static Reservations selectedReservation;
        TcpClient _client;
        public ReservationsControl(TcpClient client)
        {
            InitializeComponent();
            _client = client;
            foreach (Reservations reservation in StorageData.ReservationsList)
            {
                reservationLists.Add(reservation);
            }
            ListBoxReservations.ItemsSource = reservationLists;
            ButtonRemove.Visibility = Visibility.Hidden;
            ButtonEdit.Visibility = Visibility.Hidden;
            _client.Refresh += new TcpClient.RefreshDelegate(Refresh);
        }

        private void TextBoxClientUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            reservationLists.Clear();
            DateTimePickerDate.Value = null;
            bool containBoth = false;
            foreach (Reservations reservation in StorageData.ReservationsList)
            {
                if ((reservation.FirstName + " " + reservation.Name).Contains(TextBoxClientUser.Text) || (reservation.Name + " " + reservation.FirstName).Contains(TextBoxClientUser.Text))
                {
                    reservationLists.Add(reservation);
                    containBoth = true;
                }
            }
            if (!containBoth)
            {
                foreach (Reservations reservation in StorageData.ReservationsList)
                {
                    if (reservation.FirstName.Contains(TextBoxClientUser.Text) || reservation.Name.Contains(TextBoxClientUser.Text))
                    {
                        reservationLists.Add(reservation);
                    }
                }
            }
            ListBoxReservations.ItemsSource = null;
            ListBoxReservations.ItemsSource = reservationLists;
        }

        private void DateTimePickerDate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DateTimePickerDate.Value != null && DateTimePickerDate.Value.GetValueOrDefault().Second != 0)
            {
                DateTimePickerDate.Value = ((DateTimePickerDate.Value.GetValueOrDefault()).AddSeconds(-(DateTimePickerDate.Value.GetValueOrDefault()).Second));
            }
            else
            {
                bool containBoth = false;
                reservationLists.Clear();

                if (TextBoxClientUser.Text != "")
                {
                    foreach (Reservations reservation in StorageData.ReservationsList)
                    {
                        if ((reservation.FirstName + " " + reservation.Name).Contains(TextBoxClientUser.Text) || (reservation.Name + " " + reservation.FirstName).Contains(TextBoxClientUser.Text) && reservation.DateOn.Equals(DateTimePickerDate.Value.GetValueOrDefault()))
                        {
                            reservationLists.Add(reservation);
                            containBoth = true;
                        }
                    }
                    if (!containBoth)
                    {
                        foreach (Reservations reservation in StorageData.ReservationsList)
                        {
                            if ((reservation.FirstName.Contains(TextBoxClientUser.Text) || reservation.Name.Contains(TextBoxClientUser.Text)) && reservation.DateOn.Equals(DateTimePickerDate.Value.GetValueOrDefault()))
                            {
                                reservationLists.Add(reservation);

                            }
                        }
                    }
                }
                else
                {
                    foreach (Reservations reservation in StorageData.ReservationsList)
                    {
                        if (reservation.DateOn.Equals(DateTimePickerDate.Value.GetValueOrDefault()))
                        {
                            reservationLists.Add(reservation);
                        }
                    }
                }
                ListBoxReservations.ItemsSource = null;
                ListBoxReservations.ItemsSource = reservationLists;
            }
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _client.requestStatus = 0;
                MyMessageBox myMessageBox = new MyMessageBox(_client, 0);
                myMessageBox.ShowDialog();

                if (_client.requestStatus == 1)
                {
                    _client.SendMessage("0073<EOP>" + selectedReservation.Id.ToString() + "<EOP>");
                    _client.requestStatus = 0;
                    int time = DateTime.Now.Second;
                    while (_client.requestStatus == 0)
                    {
                        if (time - DateTime.Now.Second > 60)
                        {
                            myMessageBox = new MyMessageBox(_client, 3);
                            myMessageBox.ShowDialog();
                            return;
                        }
                    }
                    myMessageBox = new MyMessageBox(_client, _client.requestStatus);
                    myMessageBox.ShowDialog();
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            Grid ParentGrid = (Grid)this.Parent;
            ParentGrid.Children.Add(new ReservationEdit(_client, selectedReservation));
            ParentGrid.Children.RemoveAt(ParentGrid.Children.Count - 2);
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            Grid ParentGrid = (Grid)this.Parent;
            ParentGrid.Children.Add(new ReservationNew(_client));
            ParentGrid.Children.RemoveAt(ParentGrid.Children.Count - 2);
        }

        private void ListBoxReservations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxReservations.SelectedIndex > -1)
            {
                selectedReservation = (Reservations)ListBoxReservations.SelectedItem;
                ButtonRemove.Visibility = Visibility.Visible;
                ButtonEdit.Visibility = Visibility.Visible;
            }
        }

        private delegate void RefreshDelegate();
        private void Refresh()
        {
            if (!CheckAccess())
            {
                Dispatcher.BeginInvoke(new RefreshDelegate(Refresh));
                return;
            }

            reservationLists.Clear();
            foreach (Reservations reservation in StorageData.ReservationsList)
            {
                reservationLists.Add(reservation);
            }
            ListBoxReservations.ItemsSource = null;
            ListBoxReservations.ItemsSource = reservationLists;
            ButtonRemove.Visibility = Visibility.Hidden;
            ButtonEdit.Visibility = Visibility.Hidden;
        }
    }
}
