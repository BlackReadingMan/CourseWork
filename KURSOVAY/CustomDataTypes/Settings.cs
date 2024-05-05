using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Windows;

namespace CourseWork.CustomDataTypes
{
	public class Settings
	{
		private bool _isOk = true;
		public Vector3 _position { get; private set; }

		public float[] Position
		{
			get => [_position.X, _position.Y, _position.Z];
			set
			{
				try
				{
					_position = new Vector3(value);
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке Position");
					_isOk = false;
				}
			}
		}

		public Vector3 _forward { get; private set; }

		public float[] Forward
		{
			get => [_forward.X, _forward.Y, _forward.Z];
			set
			{
				try
				{
					_forward = new Vector3(value);
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке Forward");
					_isOk = false;
				}
			}
		}

		public Vector3 _up { get; private set; }

		public float[] Up
		{
			get => [_up.X, _up.Y, _up.Z];
			set
			{
				try
				{
					_up = new Vector3(value);
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке Up");
					_isOk = false;
				}
			}
		}
		public Vector3 _rotation { get; private set; }

		public float[] Rotation
		{
			get => [_rotation.X, _rotation.Y, _rotation.Z];
			set
			{
				try
				{
					_rotation = new Vector3(value);
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке Rotation");
					_isOk = false;
				}
			}
		}
		private float _scale;
		public float Scale
		{
			get => _scale;
			set
			{
				try
				{
					_scale = value;
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке Scale");
					_isOk = false;
				}
			}
		}
		public Vector3 _radiusPhiTheta { get; private set; }

		public float[] RadiusPhiTheta
		{
			get => [_radiusPhiTheta.X, _radiusPhiTheta.Y, _radiusPhiTheta.Z];
			set
			{
				try
				{
					_radiusPhiTheta = new Vector3(value);
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке RadiusPhiTheta");
					_isOk = false;
				}
			}
		}

		public Vector3 _cameraTarget { get; private set; }

		public float[] CameraTarget
		{
			get => [_cameraTarget.X, _cameraTarget.Y, _cameraTarget.Z];
			set
			{
				try
				{
					_cameraTarget = new Vector3(value);
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке CameraTarget");
					_isOk = false;
				}
			}
		}

		public Vector3 _cameraUpVector { get; private set; }

		public float[] CameraUpVector
		{
			get => [_cameraUpVector.X, _cameraUpVector.Y, _cameraUpVector.Z];
			set
			{
				try
				{
					_cameraUpVector = new Vector3(value);
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке CameraUpVector");
					_isOk = false;
				}
			}
		}
		private float _spectatorStep;
		public float SpectatorStep
		{
			get => _spectatorStep;
			set
			{
				try
				{
					_spectatorStep = value;
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке SpectatorStep");
					_isOk = false;
				}
			}
		}
		private float _fieldOfView;
		public float FieldOfView
		{
			get => _fieldOfView;
			set
			{
				try
				{
					_fieldOfView = value;
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке FieldOfView");
					_isOk = false;
				}
			}
		}
		private float _nearPlaneDistance;
		public float NearPlaneDistance
		{
			get => _nearPlaneDistance;
			set
			{
				try
				{
					_nearPlaneDistance = value;
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке NearPlaneDistance");
					_isOk = false;
				}
			}
		}
		private float _farPlaneDistance;
		public float FarPlaneDistance
		{
			get => _farPlaneDistance;
			set
			{
				try
				{
					_farPlaneDistance = value;
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке FarPlaneDistance");
					_isOk = false;
				}
			}
		}

		private float _x0;
		public float X0
		{
			get => _x0;
			set
			{
				try
				{
					_x0 = value;
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке X0");
					_isOk = false;
				}
			}
		}
		private float _y0;
		public float Y0
		{
			get => _y0;
			set
			{
				try
				{
					_y0 = value;
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке Y0");
					_isOk = false;
				}
			}
		}
		private float _minDepth;
		public float MinDepth
		{
			get => _minDepth;
			set
			{
				try
				{
					_minDepth = value;
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке MinDepth");
					_isOk = false;
				}
			}
		}
		private float _maxDepth;
		public float MaxDepth
		{
			get => _maxDepth;
			set
			{
				try
				{
					_maxDepth = value;
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке MaxDepth");
					_isOk = false;
				}
			}
		}
		private bool _lightFollowCamera;
		public bool LightFollowCamera
		{
			get => _lightFollowCamera;
			set
			{
				try
				{
					_lightFollowCamera = value;
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке LightFollowCamera");
					_isOk = false;
				}
			}
		}
		public Vector3 _lightPosition { get; private set; }

		public float[] LightPosition
		{
			get => [_lightPosition.X, _lightPosition.Y, _lightPosition.Z];
			set
			{
				try
				{
					_lightPosition = new Vector3(value);
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке LightPosition");
					_isOk = false;
				}
			}
		}
		public Vector3 _lightColor { get; private set; }

		public float[] LightColor
		{
			get => [_lightColor.X, _lightColor.Y, _lightColor.Z];
			set
			{
				try
				{
					_lightColor = new Vector3(value);
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке LightColor");
					_isOk = false;
				}
			}
		}
		public Vector3 _objectColor { get; private set; }

		public float[] ObjectColor
		{
			get => [_objectColor.X, _objectColor.Y, _objectColor.Z];
			set
			{
				try
				{
					_objectColor = new Vector3(value);
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке ObjectColor");
					_isOk = false;
				}
			}
		}
		public Vector3 _backGroundColor { get; private set; }
		public float[] BackGroundColor
		{
			get => [_backGroundColor.X, _backGroundColor.Y, _backGroundColor.Z];
			set
			{
				try
				{
					_backGroundColor = new Vector3(value);
				}
				catch
				{
					MessageBox.Show("Ошибка в настройке BackGroundColor");
					_isOk = false;
				}
			}
		}

		public static async Task<Settings?> GetSettingsAsync(string filePath)
		{
			var settings = await ReadAsync<Settings>(filePath);
			if (settings is { _isOk: false })
				settings = null;
			return settings;
		}
		private static async Task<T?> ReadAsync<T>(string filePath) where T : class
		{
			Stream assembly;
			try
			{
				assembly = Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + ".InputData." + filePath);
			}
			catch
			{
				MessageBox.Show("Файл с настройками не найден");
				return null;
			}
			try
			{
				return await JsonSerializer.DeserializeAsync<T>(utf8Json: assembly);
			}
			catch
			{
				MessageBox.Show("Ошибка в названиях настроек");
				return null;
			}
		}
	}
}
