using System;
using System.Collections.Generic;
using TiaGenerator.Core.Interfaces;

namespace TiaGenerator.Models
{
	public sealed class DataStore : IDataStore, IDisposable
	{
		public const string TiaPortalKey = "TiaPortal";
		public const string TiaProjectKey = "TiaProject";
		public const string TiaPlcDeviceKey = "TiaPlcDevice";

		private readonly Dictionary<string, object> _data = new();

		public void SetValue<T>(string name, T value)
		{
			_data[name] = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <inheritdoc />
		public T? GetValue<T>(string name)
		{
			if (_data.TryGetValue(name, out var value))
				return (T?) value;

			return default;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			foreach (var @object in _data)
			{
				if (@object.Value is IDisposable disposable)
					disposable.Dispose();
			}
		}
	}
}