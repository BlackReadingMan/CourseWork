using System.Numerics;
using System.Reflection;
using System.Text.Json;

namespace CourseWork.CustomDataTypes
{
	public class Settings
	{
		public Vector3 _position { get; private set; }

		public float[] Position
		{
			get => [_position.X, _position.Y, _position.Z];
			set => _position = new Vector3(value);
		}

		public Vector3 _forward { get; private set; }

		public float[] Forward
		{
			get => [_forward.X, _forward.Y, _forward.Z];
			set => _forward = new Vector3(value);
		}

		public Vector3 _up { get; private set; }

		public float[] Up
		{
			get => [_up.X, _up.Y, _up.Z];
			set => _up = new Vector3(value);
		}

		public Vector3 _radiusPhiTheta { get; private set; }

		public float[] RadiusPhiTheta
		{
			get => [_radiusPhiTheta.X, _radiusPhiTheta.Y, _radiusPhiTheta.Z];
			set => _radiusPhiTheta = new Vector3(value);
		}

		public Vector3 _cameraTarget { get; private set; }

		public float[] CameraTarget
		{
			get => [_cameraTarget.X, _cameraTarget.Y, _cameraTarget.Z];
			set => _cameraTarget = new Vector3(value);
		}

		public Vector3 _cameraUpVector { get; private set; }

		public float[] CameraUpVector
		{
			get => [_cameraUpVector.X, _cameraUpVector.Y, _cameraUpVector.Z];
			set => _cameraUpVector = new Vector3(value);
		}
		public float SpectatorStep { get; set; }

		public float FieldOfView { get; set; }
		public float NearPlaneDistance { get; set; }
		public float FarPlaneDistance { get; set; }

		public float X0 { get; set; }
		public float Y0 { get; set; }
		public float MinDepth { get; set; }
		public float MaxDepth { get; set; }

		public Vector3 _lightPosition { get; private set; }

		public float[] LightPosition
		{
			get => [_lightPosition.X, _lightPosition.Y, _lightPosition.Z];
			set => _lightPosition = new Vector3(value);
		}
		public Vector3 _lightColor { get; private set; }

		public float[] LightColor
		{
			get => [_lightColor.X, _lightColor.Y, _lightColor.Z];
			set => _lightColor = new Vector3(value);
		}
		public Vector3 _objectColor { get; private set; }

		public float[] ObjectColor
		{
			get => [_objectColor.X, _objectColor.Y, _objectColor.Z];
			set => _objectColor = new Vector3(value);
		}
		public float[] BackGroundColor { get; set; }

		public static async Task<Settings?> GetSettings(string filePath)
		{
			return await ReadAsync<Settings>(filePath);
		}
		private static async Task<T> ReadAsync<T>(string filePath) where T : class
		{
			return await JsonSerializer.DeserializeAsync<T>(utf8Json: Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + ".InputData." + filePath));
		}
	}
}
