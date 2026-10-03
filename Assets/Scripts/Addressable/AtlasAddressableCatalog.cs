using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

[CreateAssetMenu(fileName = "AtlasCatalog", menuName = "TCG/Addressable/Atlas Catalog")]
public sealed class AtlasAddressableCatalog : AddressableCatalog<AssetReferenceT<SpriteAtlas>> { }
