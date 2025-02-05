﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Equality;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser.TypeTree;
using AssetRipper.Core.Project;
using AssetRipper.IO.Endian;
using AssetRipper.Yaml;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.Core
{
	/// <summary>
	/// The artificial base class for all generated Unity classes
	/// </summary>
	public class UnityAssetBase : IUnityAssetBase, IDeepCloneable, IAlmostEquatable, IEquatable<UnityAssetBase>
	{
		public UnityVersion AssetUnityVersion { get; set; }
		public EndianType EndianType { get; set; }
		public TransferInstructionFlags TransferInstructionFlags { get; set; }

		public UnityAssetBase() { }
		public UnityAssetBase(LayoutInfo layout)
		{
			AssetUnityVersion = layout.Version;
			TransferInstructionFlags = layout.Flags;
		}

		public virtual void ReadEditor(AssetReader reader) => throw new NotSupportedException();

		public virtual void ReadRelease(AssetReader reader) => throw new NotSupportedException();

		public virtual void Read(AssetReader reader)
		{
			AssetUnityVersion = reader.Version;
			EndianType = reader.EndianType;
			TransferInstructionFlags = reader.Flags;
			if (reader.Flags.IsRelease())
			{
				ReadRelease(reader);
			}
			else
			{
				ReadEditor(reader);
			}
		}

		public virtual void WriteEditor(AssetWriter writer) => throw new NotSupportedException();

		public virtual void WriteRelease(AssetWriter writer) => throw new NotSupportedException();

		public virtual void Write(AssetWriter writer)
		{
			if (writer.Flags.IsRelease())
			{
				WriteRelease(writer);
			}
			else
			{
				WriteEditor(writer);
			}
		}

		public virtual YamlNode ExportYamlEditor(IExportContainer container) => throw new NotSupportedException($"Editor yaml export is not supported for {GetType().FullName}");

		public virtual YamlNode ExportYamlRelease(IExportContainer container) => throw new NotSupportedException($"Release yaml export is not supported for {GetType().FullName}");

		public virtual YamlNode ExportYaml(IExportContainer container)
		{
			if (container.ExportFlags.IsRelease())
			{
				//if(this.TransferInstructionFlags.IsRelease())
				return ExportYamlRelease(container);
			}
			else
			{
				return ExportYamlEditor(container);
			}
		}

		public virtual IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			return Array.Empty<PPtr<IUnityObjectBase>>();
		}

		public virtual List<TypeTreeNode> MakeReleaseTypeTreeNodes(int depth, int startingIndex) => throw new NotSupportedException();

		public virtual List<TypeTreeNode> MakeEditorTypeTreeNodes(int depth, int startingIndex) => throw new NotSupportedException();

		/// <inheritdoc/>
		public UnityAssetBase DeepClone()
		{
			UnityAssetBase asset = CreateAnother();
			asset.CopyValuesFrom(this);
			return asset;
		}

		/// <summary>
		/// Create another instance of this asset's type
		/// </summary>
		/// <returns>A new blank instance</returns>
		public virtual UnityAssetBase CreateAnother() => new UnityAssetBase();

		/// <summary>
		/// Deep copy the values of another asset
		/// </summary>
		/// <remarks>
		/// This method assumes that the other asset is not null
		/// and has the same type as this.
		/// </remarks>
		/// <param name="source">The source asset</param>
		protected virtual void CopyValuesFrom(UnityAssetBase source)
		{
			AssetUnityVersion = source.AssetUnityVersion;
			EndianType = source.EndianType;
			TransferInstructionFlags = source.TransferInstructionFlags;
		}

		private bool HasEqualMetadata([NotNullWhen(true)] object? obj)
		{
			if(obj is null)
			{
				return false;
			}

			if (this.GetType() != obj.GetType())
			{
				return false;
			}

			if (obj is UnityObjectBase unityObjectBase)
			{
				UnityObjectBase thisObject = (UnityObjectBase)this;
				return thisObject.AssetUnityVersion == unityObjectBase.AssetUnityVersion &&
					//thisObject.EndianType == unityObjectBase.EndianType &&
					//thisObject.TransferInstructionFlags == unityObjectBase.TransferInstructionFlags &&
					thisObject.SerializedFile == unityObjectBase.SerializedFile &&
					thisObject.ClassID == unityObjectBase.ClassID &&
					thisObject.PathID == unityObjectBase.PathID &&
					thisObject.GUID == unityObjectBase.GUID;
			}
			else
			{
				UnityAssetBase asset = (UnityAssetBase)obj;
				return 
					//this.TransferInstructionFlags == asset.TransferInstructionFlags &&
					//this.EndianType == asset.EndianType &&
					this.AssetUnityVersion == asset.AssetUnityVersion;
			}
		}

		/// <summary>
		/// Check if another asset is equal to this
		/// </summary>
		/// <remarks>
		/// This method assumes that the other asset is not null
		/// and has the same metadata as this.
		/// </remarks>
		/// <param name="other">Another asset</param>
		/// <returns></returns>
		public bool Equals(UnityAssetBase? other)
		{
			return HasEqualMetadata(other) && EqualByContent(other);
		}

		/// <summary>
		/// Check if another asset is equal to this
		/// </summary>
		/// <remarks>
		/// This method assumes that the other asset is not null
		/// and has the same metadata as this.
		/// </remarks>
		/// <param name="other">Another asset</param>
		/// <returns></returns>
		protected virtual bool EqualByContent(UnityAssetBase other)
		{
			return ReferenceEquals(this, other);
		}

		/// <inheritdoc/>
		public bool AlmostEqualByProportion(object value, float maximumProportion)
		{
			if (HasEqualMetadata(value))
			{
				return AlmostEqualByProportion((UnityAssetBase)value, maximumProportion);
			}
			else
			{
				return false;
			}
		}

		/// <inheritdoc/>
		public bool AlmostEqualByDeviation(object value, float maximumDeviation)
		{
			if (HasEqualMetadata(value))
			{
				return AlmostEqualByDeviation((UnityAssetBase)value, maximumDeviation);
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Check if two objects are almost equal to each other by proportion
		/// </summary>
		/// <remarks>
		/// This method assumes that the other asset is not null
		/// and has the same metadata as this.
		/// </remarks>
		/// <param name="value">Another asset with the same type as this</param>
		/// <param name="maximumProportion"></param>
		/// <returns>True if the objects are equal or almost equal by proportion</returns>
		protected virtual bool AlmostEqualByProportion(UnityAssetBase value, float maximumProportion)
		{
			return ReferenceEquals(this, value);
		}

		/// <summary>
		/// Check if two objects are almost equal to each other by deviation
		/// </summary>
		/// <remarks>
		/// This method assumes that the other asset is not null
		/// and has the same metadata as this.
		/// </remarks>
		/// <param name="value">Another asset with the same type as this</param>
		/// <param name="maximumDeviation">The positive maximum value deviation between two near equal decimal values</param>
		/// <returns>True if the objects are equal or almost equal by deviation</returns>
		protected virtual bool AlmostEqualByDeviation(UnityAssetBase value, float maximumDeviation)
		{
			return ReferenceEquals(this, value);
		}
	}
}
