Imports Microsoft.VisualBasic
Imports System.Windows.Controls
Imports DevExpress.Xpf.Map
Imports System.Collections.ObjectModel
Imports System.Windows
Imports System.Windows.Input
Imports System.Collections.Generic

Namespace RouteProvider
	Partial Public Class MainPage
		Inherits UserControl
		Private waypointIndex As Integer
		Private waypoints As List(Of RouteWaypoint)
		Private helpers_Renamed As ObservableCollection(Of MapItem)

		Public ReadOnly Property Helpers() As ObservableCollection(Of MapItem)
			Get
				Return Me.helpers_Renamed
			End Get
		End Property

		Public Sub New()
			InitializeComponent()
			DataContext = Me
			Me.helpers_Renamed = New ObservableCollection(Of MapItem)()
			Me.waypoints = New List(Of RouteWaypoint)()
		End Sub
		Private Function NextWaypointLetter() As String
			Dim letter As String = "" & ChrW(CByte(AscW("A"c)) + waypointIndex Mod 26)
			waypointIndex += 1
			Return letter
		End Function

		Private Sub GeocodeLayerItemsGenerating(ByVal sender As Object, ByVal args As LayerItemsGeneratingEventArgs)
			For Each item As MapItem In args.Items
				Dim pushpin As MapPushpin = TryCast(item, MapPushpin)
				If pushpin IsNot Nothing Then
					AddHandler pushpin.MouseLeftButtonDown, AddressOf PushpinMouseLeftButtonDown
				End If
			Next item
		End Sub

		Private Sub PushpinMouseLeftButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
			Dim pushpin As MapPushpin = TryCast(sender, MapPushpin)
			If (pushpin IsNot Nothing) AndAlso (pushpin.State= MapPushpinState.Normal) Then
				Dim locationInformation As LocationInformation = TryCast(pushpin.Information, LocationInformation)
				AddWaypoint(If(locationInformation Is Nothing, String.Empty, locationInformation.DisplayName), pushpin.Location)
				geocodeInformationLayer.ClearResults()
			End If
			e.Handled = True
		End Sub

		Private Sub AddWaypoint(ByVal description As String, ByVal location As GeoPoint)
			Dim waypoint As New RouteWaypoint(description, location)
			If (Not waypoints.Contains(waypoint)) Then
				Dim pushpin As New MapPushpin()
				pushpin.Location = location
				pushpin.Information = description
				pushpin.Text = NextWaypointLetter()
				pushpin.TraceDepth = 0
				pushpin.State = MapPushpinState.Busy
				Helpers.Add(pushpin)
				waypoints.Add(waypoint)

				If waypoints.Count > 1 Then
					routeDataProvider.CalculateRoute(waypoints)
				End If
			End If
		End Sub

		Private Sub RouteLayerItemsGenerating(ByVal sender As Object, ByVal args As LayerItemsGeneratingEventArgs)
			If (args.Error Is Nothing) AndAlso (Not args.Cancelled) Then
				waypointIndex = 0
				For Each item As MapItem In args.Items
					Dim pushpin As MapPushpin = TryCast(item, MapPushpin)
					If pushpin IsNot Nothing Then
						pushpin.Text = NextWaypointLetter()
					End If
				Next item
				Helpers.Clear()
			End If
		End Sub

		Private Sub ClearClick(ByVal sender As Object, ByVal e As RoutedEventArgs)
			Helpers.Clear()
			waypoints.Clear()
			waypointIndex = 0
			routeInformationLayer.ClearResults()
			geocodeInformationLayer.ClearResults()
		End Sub

	End Class
End Namespace
