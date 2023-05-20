using System;
using System.Collections.Generic;
using Siemens.Engineering;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Tia.Models;

namespace TiaGenerator.Models
{
	/// <summary>
	/// A datastore to store the data that is used in the application and between actions
	/// </summary>
	public sealed class DataStore : IDataStore, IDisposable
	{
		/// <summary>
		/// Used to store the data as key value pairs
		/// </summary>
		private readonly Dictionary<string, object> _data = new();

		/// <summary>
		/// Get the tia portal instance, when there is one
		/// </summary>
		public TiaPortal? TiaPortal
		{
			get => GetValue<TiaPortal>(nameof(TiaPortal));
			set => SetValue(nameof(TiaPortal), value);
		}

		/// <summary>
		/// Get the tia project instance, when there is one
		/// </summary>
		public Project? TiaProject
		{
			get => GetValue<Project>(nameof(TiaProject));
			set => SetValue(nameof(TiaProject), value);
		}

		/// <summary>
		/// Get the tia plc device instance, when there is one
		/// </summary>
		public PlcDevice? TiaPlcDevice
		{
			get => GetValue<PlcDevice>(nameof(TiaPlcDevice));
			set => SetValue(nameof(TiaPlcDevice), value);
		}

		/// <summary>
		/// Get the plc block group, when there is one
		/// </summary>
		public PlcBlockUserGroup? PlcBlockGroup
		{
			get => GetValue<PlcBlockUserGroup>(nameof(PlcBlockGroup));
			set => SetValue(nameof(PlcBlockGroup), value);
		}

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