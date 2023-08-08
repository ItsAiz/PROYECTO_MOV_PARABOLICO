' Importar los espacios de nombres necesarios
Imports System.Drawing
Imports System.Windows.Forms

Public Class Form1
    ' Evento de carga del formulario
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Configurar las columnas de los dataGridViews
        dgvResultados.Columns.Add("Ángulo", "Ángulo")
        dgvResultados.Columns.Add("Velocidad", "Velocidad Inicial")

        dgvMaxAltitude.Columns.Add("Ángulo", "Ángulo")
        dgvMaxAltitude.Columns.Add("Velocidad", "Velocidad Inicial")
        dgvMaxAltitude.Columns.Add("Tiempo de Vuelo hasta intersección", "Tiempo de Vuelo hasta intersección")
        dgvMaxAltitude.Columns.Add("Tiempo trayectoria", "Tiempo trayectoria")
        dgvMaxAltitude.Columns.Add("Coordenada Final", "Coordenada Final")
        dgvMaxAltitude.Columns.Add("Altura Máxima", "Altura Máxima")
        dgvMaxAltitude.Columns.Add("Altura Máxima hasta intersección", "Altura Máxima hasta intersección")
        dgvMaxAltitude.Columns.Add("Distancia Recorrida", "Distancia Recorrida")
        dgvMaxAltitude.Columns.Add("Distancia Recorrida hasta intersección", "Distancia Recorrida hasta intersección")

        dgvMinAltitude.Columns.Add("Ángulo", "Ángulo")
        dgvMinAltitude.Columns.Add("Velocidad", "Velocidad Inicial")
        dgvMinAltitude.Columns.Add("Tiempo de Vuelo hasta intersección", "Tiempo de Vuelo hasta intersección")
        dgvMinAltitude.Columns.Add("Tiempo trayectoria", "Tiempo trayectoria")
        dgvMinAltitude.Columns.Add("Coordenada Final", "Coordenada Final")
        dgvMinAltitude.Columns.Add("Altura Máxima", "Altura Máxima")
        dgvMinAltitude.Columns.Add("Altura Máxima hasta intersección", "Altura Máxima hasta intersección")
        dgvMinAltitude.Columns.Add("Distancia Recorrida", "Distancia Recorrida")
        dgvMinAltitude.Columns.Add("Distancia Recorrida hasta intersección", "Distancia Recorrida hasta intersección")
    End Sub
    ' Evento del botón Calcular para iniciar la simulación
    Private Sub btnCalcular_Click(sender As Object, e As EventArgs) Handles btnCalcular.Click
        ' Instancia generador de números aleatorios
        Dim random As New Random()
        ' Limpiar las tablas de resultados
        dgvResultados.Rows.Clear()
        dgvMaxAltitude.Rows.Clear()
        dgvMinAltitude.Rows.Clear()
        ' Consideraciones iniciales para la visualización de las trayectorias
        PictureBox1.Image = New Bitmap(PictureBox1.Width, PictureBox1.Height)
        Dim g As Graphics = Graphics.FromImage(PictureBox1.Image)
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        ' Contador de trayectorias que intersectan el círculo
        Dim intersectCount As Integer = 0

        ' Consideraciones iniciales para generar la posición aleatoria del círculo
        Dim circleX As Integer = random.Next(200, 501)
        Dim circleY As Integer = random.Next(100, 301)
        Dim circleRadius As Integer = 5

        ' Variables para almacenar los datos de las trayectorias
        Dim maxAltitudeData As TrajectoryData = Nothing
        Dim minAltitudeData As TrajectoryData = Nothing
        Dim maxIntersectAltitude As Double = Double.MinValue
        Dim minIntersectAltitude As Double = Double.MaxValue
        Dim trajectories As New List(Of TrajectoryData)

        ' Llamado a función para dibujar el circulo aleatorio teniendo en cuenta las diferencias de alturas reales
        DrawCircle(g, circleX, PictureBox1.Height - circleY, circleRadius)

        ' Ciclos necesarios para iterar sobre diferentes ángulos y velocidades en el rango establecido
        For angle As Integer = 30 To 60
            For speed As Integer = 50 To 100
                ' Cálculo del ángulo en radianes y el tiempo de vuelo
                Dim radianAngle As Double = angle * Math.PI / 180.0
                Dim timeOfFlight As Double = (2 * speed * Math.Sin(radianAngle)) / 9.81

                ' Lista para almacenar los puntos de la trayectoria para poder ser dibujadas
                Dim pointsToUpdate As New List(Of Point)
                Dim trajectory As New TrajectoryData()

                ' Calculos necesarios para dibujar la trayectoria y cumplir las consideraciones dispuestas frente a los datos a mostrar
                For t As Double = 0 To timeOfFlight Step 0.01
                    Dim x = speed * Math.Cos(radianAngle) * t
                    Dim y = speed * Math.Sin(radianAngle) * t - (0.5 * 9.81 * t * t)

                    pointsToUpdate.Add(New Point(CInt(x), PictureBox1.Height - CInt(y)))
                    ' Condicional para verificar si la trayectoria intersecta el círculo
                    If IsIntersectingCircle(x, y, circleX, circleY, circleRadius) Then
                        dgvResultados.Rows.Add(angle, speed)
                        intersectCount += 1

                        ' Almacenar datos de la trayectoria actual
                        trajectory.Angle = angle
                        trajectory.Speed = speed
                        trajectory.TimeOfFlight = t
                        trajectory.CoordinateFinal = New Point(CInt(x), PictureBox1.Height - CInt(y))
                        trajectory.MaxAltitude = (speed ^ 2 * Math.Sin(radianAngle) ^ 2) / (2 * 9.81)
                        trajectory.Distance = (speed ^ 2 * Math.Sin(2 * radianAngle)) / 9.81
                        trajectory.MaxAltitudeIntersect = y
                        trajectory.DistanceIntersect = x
                        trajectory.TimeOfFlightTotal = (2 * speed * Math.Sin(angle * (Math.PI / 180))) / 9.81

                        trajectories.Add(trajectory)
                        ' Trayectoria que interseca con la máxima altura
                        If y > maxIntersectAltitude Then
                            maxIntersectAltitude = y
                            maxAltitudeData = trajectory
                        End If
                        ' Trayectoria que interseca con la mínima altura
                        If y < minIntersectAltitude Then
                            minIntersectAltitude = y
                            minAltitudeData = trajectory
                        End If

                        Exit For
                    End If
                Next
                ' Llamado función/método para dibujar la trayectoria actual en el pictureBox y consideraciones para visualización en tiempo real
                DrawTrajectory(g, pointsToUpdate)
                PictureBox1.Refresh()
                Application.DoEvents()
            Next
        Next
        ' Instancias y llamados para mostrar los resultados
        DisplayResults(maxAltitudeData, minAltitudeData, circleX, circleY, circleRadius)
        MessageBox.Show($"Trayectorias que intersectan con el círculo: {intersectCount}")
    End Sub

    ' Método para mostrar los resultados en los dataGridViews correspondientes
    Private Sub DisplayResults(maxAltitudeData As TrajectoryData, minAltitudeData As TrajectoryData, circleX As Integer, circleY As Integer, circleRadius As Integer)
        If maxAltitudeData IsNot Nothing Then
            dgvMaxAltitude.Rows.Add(maxAltitudeData.Angle, maxAltitudeData.Speed, maxAltitudeData.TimeOfFlight, maxAltitudeData.TimeOfFlightTotal,
                                     maxAltitudeData.CoordinateFinal, maxAltitudeData.MaxAltitude, maxAltitudeData.MaxAltitudeIntersect,
                                     maxAltitudeData.Distance, maxAltitudeData.DistanceIntersect)
            DisplayTrajectory(maxAltitudeData, PictureBox2, circleX, circleY, circleRadius)
        End If

        If minAltitudeData IsNot Nothing Then
            dgvMinAltitude.Rows.Add(minAltitudeData.Angle, minAltitudeData.Speed, minAltitudeData.TimeOfFlight, minAltitudeData.TimeOfFlightTotal,
                                     minAltitudeData.CoordinateFinal, minAltitudeData.MaxAltitude, minAltitudeData.MaxAltitudeIntersect,
                                     minAltitudeData.Distance, minAltitudeData.DistanceIntersect)
            DisplayTrajectory(minAltitudeData, PictureBox3, circleX, circleY, circleRadius)
        End If
    End Sub
    ' Método para mostrar la trayectoria en un PictureBox
    Private Sub DisplayTrajectory(trajectory As TrajectoryData, pictureBox As PictureBox, circleX As Integer, circleY As Integer, circleRadius As Integer)
        pictureBox.Image = New Bitmap(pictureBox.Width, pictureBox.Height)
        Dim g As Graphics = Graphics.FromImage(pictureBox.Image)
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias

        DrawCircle(g, circleX, pictureBox.Height - circleY, circleRadius)

        Dim pointsToUpdate As New List(Of Point)

        For t As Double = 0 To trajectory.TimeOfFlight Step 0.01
            Dim x = trajectory.Speed * Math.Cos(trajectory.Angle * Math.PI / 180.0) * t
            Dim y = trajectory.Speed * Math.Sin(trajectory.Angle * Math.PI / 180.0) * t - (0.5 * 9.81 * t * t)

            pointsToUpdate.Add(New Point(CInt(x), pictureBox.Height - CInt(y)))

            If IsIntersectingCircle(x, y, circleX, circleY, circleRadius) Then
                Exit For
            End If
        Next

        DrawTrajectory(g, pointsToUpdate)
        pictureBox.Refresh()
    End Sub
    ' Método para dibujar la trayectoria en el pictureBox
    Private Sub DrawTrajectory(g As Graphics, points As List(Of Point))
        Dim pen As New Pen(Color.Blue)
        g.DrawLines(pen, points.ToArray())
    End Sub
    ' Método para dibujar el círculo con coordenadas aleatorias y radio de 5
    Private Sub DrawCircle(g As Graphics, x As Integer, y As Integer, radius As Integer)
        Dim pen As New Pen(Color.Red)
        g.DrawEllipse(pen, x - radius, y - radius, radius * 2, radius * 2)
    End Sub
    ' Método para verificar si la trayectoria intersecta el círculo
    Private Function IsIntersectingCircle(x As Double, y As Double, circleX As Integer, circleY As Integer, circleRadius As Integer) As Boolean
        Dim horizontalDistance As Double = x - circleX
        Dim verticalDistance As Double = y - circleY

        Return Math.Sqrt(horizontalDistance ^ 2 + verticalDistance ^ 2) <= circleRadius
    End Function

End Class
' Clase para almacenar datos de las trayectorias
Public Class TrajectoryData
    Public Property Angle As Integer
    Public Property Speed As Integer
    Public Property TimeOfFlight As Double
    Public Property TimeOfFlightTotal As Double
    Public Property CoordinateFinal As Point
    Public Property MaxAltitude As Double
    Public Property Distance As Double
    Public Property MaxAltitudeIntersect As Double
    Public Property DistanceIntersect As Double
End Class
