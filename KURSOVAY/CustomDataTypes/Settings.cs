using Newtonsoft.Json;
using System.IO;
using System.Numerics;
using System.Windows;

namespace CourseWork.CustomDataTypes;

public class Settings
{
	public Vector3 Position { get; set; }
	public Vector3 Forward { get; set; }
	public Vector3 Up { get; set; } = new(0, 1f, 0);
	public float Scale { get; set; }
	public Vector3 RadiusPhiTheta { get; set; }
	public Vector3 CameraTarget { get; set; }
	public Vector3 CameraUpVector { get; set; }
	public Vector3 SpectatorStep { get; set; }
	public float FieldOfView { get; set; }
	public float NearPlaneDistance { get; set; }
	public float FarPlaneDistance { get; set; }
	public float X0 { get; set; }
	public float Y0 { get; set; }
	public float MinDepth { get; set; }
	public float MaxDepth { get; set; }

	public bool IsLightFollowsCamera { get; set; }
	public Vector3 LightPosition { get; set; }
	public Vector3 LightColor { get; set; }
	public Vector3 ObjectColor { get; set; }
	public Vector3 BackGroundColor { get; set; }


	public static async Task<Settings?> GetSettingsAsync(string filePath)
	{
		var settings = await ReadAsync<Settings>(filePath);
		return settings;
	}
	private static async Task<T?> ReadAsync<T>(string filePath) where T : class
	{
		string json;
		try
		{
			json = await File.ReadAllTextAsync(filePath);
		}
		catch
		{
			MessageBox.Show("Файл с настройками не найден");
			return null;
		}
		try
		{
			return JsonConvert.DeserializeObject<T>(json);
		}
		catch
		{
			MessageBox.Show("Ошибка в названиях настроек");
			return null;
		}
	}
}