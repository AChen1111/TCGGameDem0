using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"LitMotion.Extensions.dll",
		"LitMotion.dll",
		"Newtonsoft.Json.dll",
		"System.Core.dll",
		"System.dll",
		"UniTask.dll",
		"Unity.Addressables.dll",
		"Unity.InputSystem.dll",
		"Unity.ResourceManager.dll",
		"UnityEngine.CoreModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Networking.AuthApi.<ClaimGiftAsync>d__23,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Networking.AuthApi.<DrawCardsAsync>d__9,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Networking.AuthApi.<GetFriendsAsync>d__17,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Networking.AuthApi.<GetGachaPoolAsync>d__10,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Networking.AuthApi.<GetInboxAsync>d__22,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Networking.AuthApi.<GetPlayerAsync>d__6,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Networking.AuthApi.<PostAuthAsync>d__24,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Networking.AuthApi.<PurchaseShopItemAsync>d__8,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Networking.AuthApi.<SearchFriendsAsync>d__18,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Networking.AuthApi.<UpdateProfileAsync>d__7,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Networking.BackendHttpClient.<SendAsync>d__4,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Networking.BackendHttpClient.<SendAsync>d__5<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Networking.BackendHttpClient.<SendRawAsync>d__6,AChen.Networking.BackendHttpResponse>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Networking.ContentUpdatePrompt.<WaitForRetryAsync>d__10>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Networking.LocalGameConfiguration.<CheckVersionAsync>d__9>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Networking.LocalGameConfiguration.<InitializeAsync>d__8>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Player.AuthFlow.<AuthenticateAsync>d__3,AChen.Player.AuthResult>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Player.PlayerSession.<>c__DisplayClass38_0.<<SendFriendRequestAsync>b__0>d,byte>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Player.PlayerSession.<>c__DisplayClass39_0.<<AcceptFriendRequestAsync>b__0>d,byte>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Player.PlayerSession.<>c__DisplayClass40_0.<<RejectFriendRequestAsync>b__0>d,byte>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Player.PlayerSession.<ExecuteLockedAsync>d__47<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Player.PlayerSession.<GetGachaPoolAsync>d__43,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Player.PlayerSession.<LoginAsync>d__25,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Player.PlayerSession.<LogoutAsync>d__28>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Player.PlayerSession.<RefreshAsync>d__27>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Player.PlayerSession.<RegisterAsync>d__24,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Player.PlayerSession.<SendAuthenticatedAsync>d__49,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Player.PlayerSession.<SendAuthenticatedCallAsync>d__48<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Player.PlayerSession.<SendAuthenticatedDrawAsync>d__51,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Player.PlayerSession.<SendAuthenticatedPoolAsync>d__50,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AChen.Player.PlayerSession.<TryRestoreSessionAsync>d__26,byte>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AUIScreenController.<RunGuardedAsync>d__46,byte>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AUIScreenController.<RunTransitionAsync>d__70>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AddressableLoader.<AwaitScene>d__29,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AddressableLoader.<LoadAsset>d__28<object,object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AddressableLoader.<LoadScene>d__21,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AddressableLoader.<ReloadScene>d__22,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AddressableLoader.<UnloadScene>d__26>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<AvatarSelectWindow.<BindListAsync>d__24>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<CardPackShopCategory.<BindAsync>d__3>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<CardPickController.<DoEndShow>d__30>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<CardPoolAddress.<LoadAddressAsync>d__7,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<CardPoolAddress.<LoadCardTextureAsync>d__6,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<CardPoolAddress.<TryLoadAddressAsync>d__8,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<CardPreviewWindow.<LoadCardImageAsync>d__16,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<CardPreviewWindow.<LoadCardImagesAsync>d__15,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<CardZoomWindow.<AnimateBowAsync>d__31>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<CardZoomWindow.<PlayCloseBowAsync>d__30>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<CardZoomWindow.<PlayOpenBowAsync>d__29>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<CosmeticShopCategory.<BindAsync>d__9>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<FriendWindow.<<LoadFriendsAsync>b__14_0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<FriendWindow.<>c__DisplayClass15_0.<<SearchAsync>b__0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<FriendWindow.<>c__DisplayClass16_0.<<SendRequestAsync>b__0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<FriendWindow.<BindRowsAsync>d__17>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<GameFlow.<CheckContentAsync>d__4>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<GameFlow.<GetStartupSceneAsync>d__10,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<GiftWindow.<<LoadInboxAsync>b__9_0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<GiftWindow.<>c__DisplayClass10_0.<<AcceptAsync>b__0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<GiftWindow.<>c__DisplayClass11_0.<<RejectAsync>b__0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<GiftWindow.<>c__DisplayClass12_0.<<ClaimAsync>b__0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<GiftWindow.<RefreshInboxAsync>d__13>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<GridListController.<LoadRowPrefabAsync>d__13,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<InboxListController.<BindAsync>d__6>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<LobbyWallpaperView.<LoadAsync>d__16>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<LobbyWallpaperView.<PlayRevealAsync>d__15>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<LobbyWallpaperView.<WaitForReadyAsync>d__10>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<LogInWindow.<DoAnim>d__32>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<PlayerProfileView.<LoadAvatarAsync>d__9>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<PreGameUIPanel.<<SwitchToNextWallpaperAsync>b__38_0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<PreGameUIPanel.<PlayIntroAsync>d__39>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<SceneLoader.<LoadScene>d__1,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<SceneLoader.<ReloadScene>d__2,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<ShopCatalogQuery.<LoadCardPacksAsync>d__0,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<ShopCatalogQuery.<LoadCosmeticsAsync>d__2<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<ShopCatalogQuery.<LoadSpriteAsync>d__6,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<ShopCatalogQuery.<LoadSpritesAsync>d__5,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<ShopWindow.<LoadDrawCardAsync>d__36,CardPickViewData>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<ShopWindow.<LoadDrawCardsAsync>d__35,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<UITween.<RunAsync>d__7>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<UpdateDetector.<DownloadAssets>d__5>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<UpdateDetector.<InitializeLocalAsync>d__4>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Networking.AuthApi.<ClaimGiftAsync>d__23,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Networking.AuthApi.<DrawCardsAsync>d__9,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Networking.AuthApi.<GetFriendsAsync>d__17,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Networking.AuthApi.<GetGachaPoolAsync>d__10,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Networking.AuthApi.<GetInboxAsync>d__22,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Networking.AuthApi.<GetPlayerAsync>d__6,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Networking.AuthApi.<PostAuthAsync>d__24,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Networking.AuthApi.<PurchaseShopItemAsync>d__8,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Networking.AuthApi.<SearchFriendsAsync>d__18,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Networking.AuthApi.<UpdateProfileAsync>d__7,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Networking.BackendHttpClient.<SendAsync>d__4,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Networking.BackendHttpClient.<SendAsync>d__5<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Networking.BackendHttpClient.<SendRawAsync>d__6,AChen.Networking.BackendHttpResponse>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Networking.ContentUpdatePrompt.<WaitForRetryAsync>d__10>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Networking.LocalGameConfiguration.<CheckVersionAsync>d__9>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Networking.LocalGameConfiguration.<InitializeAsync>d__8>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Player.AuthFlow.<AuthenticateAsync>d__3,AChen.Player.AuthResult>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Player.PlayerSession.<>c__DisplayClass38_0.<<SendFriendRequestAsync>b__0>d,byte>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Player.PlayerSession.<>c__DisplayClass39_0.<<AcceptFriendRequestAsync>b__0>d,byte>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Player.PlayerSession.<>c__DisplayClass40_0.<<RejectFriendRequestAsync>b__0>d,byte>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Player.PlayerSession.<ExecuteLockedAsync>d__47<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Player.PlayerSession.<GetGachaPoolAsync>d__43,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Player.PlayerSession.<LoginAsync>d__25,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Player.PlayerSession.<LogoutAsync>d__28>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Player.PlayerSession.<RefreshAsync>d__27>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Player.PlayerSession.<RegisterAsync>d__24,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Player.PlayerSession.<SendAuthenticatedAsync>d__49,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Player.PlayerSession.<SendAuthenticatedCallAsync>d__48<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Player.PlayerSession.<SendAuthenticatedDrawAsync>d__51,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Player.PlayerSession.<SendAuthenticatedPoolAsync>d__50,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AChen.Player.PlayerSession.<TryRestoreSessionAsync>d__26,byte>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AUIScreenController.<RunGuardedAsync>d__46,byte>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AUIScreenController.<RunTransitionAsync>d__70>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AddressableLoader.<AwaitScene>d__29,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AddressableLoader.<LoadAsset>d__28<object,object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AddressableLoader.<LoadScene>d__21,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AddressableLoader.<ReloadScene>d__22,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AddressableLoader.<UnloadScene>d__26>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<AvatarSelectWindow.<BindListAsync>d__24>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<CardPackShopCategory.<BindAsync>d__3>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<CardPickController.<DoEndShow>d__30>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<CardPoolAddress.<LoadAddressAsync>d__7,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<CardPoolAddress.<LoadCardTextureAsync>d__6,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<CardPoolAddress.<TryLoadAddressAsync>d__8,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<CardPreviewWindow.<LoadCardImageAsync>d__16,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<CardPreviewWindow.<LoadCardImagesAsync>d__15,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<CardZoomWindow.<AnimateBowAsync>d__31>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<CardZoomWindow.<PlayCloseBowAsync>d__30>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<CardZoomWindow.<PlayOpenBowAsync>d__29>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<CosmeticShopCategory.<BindAsync>d__9>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<FriendWindow.<<LoadFriendsAsync>b__14_0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<FriendWindow.<>c__DisplayClass15_0.<<SearchAsync>b__0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<FriendWindow.<>c__DisplayClass16_0.<<SendRequestAsync>b__0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<FriendWindow.<BindRowsAsync>d__17>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<GameFlow.<CheckContentAsync>d__4>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<GameFlow.<GetStartupSceneAsync>d__10,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<GiftWindow.<<LoadInboxAsync>b__9_0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<GiftWindow.<>c__DisplayClass10_0.<<AcceptAsync>b__0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<GiftWindow.<>c__DisplayClass11_0.<<RejectAsync>b__0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<GiftWindow.<>c__DisplayClass12_0.<<ClaimAsync>b__0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<GiftWindow.<RefreshInboxAsync>d__13>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<GridListController.<LoadRowPrefabAsync>d__13,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<InboxListController.<BindAsync>d__6>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<LobbyWallpaperView.<LoadAsync>d__16>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<LobbyWallpaperView.<PlayRevealAsync>d__15>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<LobbyWallpaperView.<WaitForReadyAsync>d__10>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<LogInWindow.<DoAnim>d__32>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<PlayerProfileView.<LoadAvatarAsync>d__9>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<PreGameUIPanel.<<SwitchToNextWallpaperAsync>b__38_0>d>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<PreGameUIPanel.<PlayIntroAsync>d__39>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<SceneLoader.<LoadScene>d__1,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<SceneLoader.<ReloadScene>d__2,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<ShopCatalogQuery.<LoadCardPacksAsync>d__0,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<ShopCatalogQuery.<LoadCosmeticsAsync>d__2<object>,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<ShopCatalogQuery.<LoadSpriteAsync>d__6,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<ShopCatalogQuery.<LoadSpritesAsync>d__5,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<ShopWindow.<LoadDrawCardAsync>d__36,CardPickViewData>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<ShopWindow.<LoadDrawCardsAsync>d__35,object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<UITween.<RunAsync>d__7>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<UpdateDetector.<DownloadAssets>d__5>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<UpdateDetector.<InitializeLocalAsync>d__4>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<AChen.Networking.BackendHttpResponse>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<AChen.Player.AuthResult>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<CardPickViewData>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<byte>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<AddressableLoader.<LoadCatalogsAsync>d__17>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<AvatarSelectWindow.<SubmitAsync>d__29>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<CardPreviewWindow.<LoadPoolAsync>d__14>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<CardZoomWindow.<PlayCloseAsync>d__28>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<CardZoomWindow.<PlayOpenAsync>d__27>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<ChangeNameWindow.<SubmitAsync>d__12>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<FriendWindow.<LoadFriendsAsync>d__14>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<FriendWindow.<SearchAsync>d__15>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<FriendWindow.<SendRequestAsync>d__16>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<GameFlow.<EnterLobbyAsync>d__11>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<GameFlow.<ReturnToLoginAsync>d__7>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<GiftRewardItem.<LoadArtAsync>d__6>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<GiftWindow.<AcceptAsync>d__10>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<GiftWindow.<ClaimAsync>d__12>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<GiftWindow.<LoadInboxAsync>d__9>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<GiftWindow.<RejectAsync>d__11>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<HotUpdateEntry.<BootAsync>d__2>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<LogInWindow.<AuthenticateAsync>d__20>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<LogInWindow.InputFeedback.<PlayAsync>d__6>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<MessageWindow.<CloseAfterAsync>d__9>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<MessageWindow.<PlayOpenAsync>d__5>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<PreGameUIPanel.<SwitchToNextWallpaperAsync>d__38>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<SceneEntry.<Start>d__3>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<SettingsWindow.<SwitchLanguageAsync>d__11>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<ShopWindow.<DrawPackAsync>d__34>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<ShopWindow.<PurchaseAsync>d__33>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<ShopWindow.<RefreshCurrentCategoryAsync>d__30>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<SingletonManager.<Start>d__2>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<SocialPortrait.<ApplyAsync>d__0>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<AddressableLoader.<LoadCatalogsAsync>d__17>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<AvatarSelectWindow.<SubmitAsync>d__29>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<CardPreviewWindow.<LoadPoolAsync>d__14>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<CardZoomWindow.<PlayCloseAsync>d__28>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<CardZoomWindow.<PlayOpenAsync>d__27>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<ChangeNameWindow.<SubmitAsync>d__12>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<FriendWindow.<LoadFriendsAsync>d__14>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<FriendWindow.<SearchAsync>d__15>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<FriendWindow.<SendRequestAsync>d__16>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<GameFlow.<EnterLobbyAsync>d__11>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<GameFlow.<ReturnToLoginAsync>d__7>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<GiftRewardItem.<LoadArtAsync>d__6>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<GiftWindow.<AcceptAsync>d__10>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<GiftWindow.<ClaimAsync>d__12>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<GiftWindow.<LoadInboxAsync>d__9>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<GiftWindow.<RejectAsync>d__11>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<HotUpdateEntry.<BootAsync>d__2>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<LogInWindow.<AuthenticateAsync>d__20>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<LogInWindow.InputFeedback.<PlayAsync>d__6>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<MessageWindow.<CloseAfterAsync>d__9>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<MessageWindow.<PlayOpenAsync>d__5>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<PreGameUIPanel.<SwitchToNextWallpaperAsync>d__38>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<SceneEntry.<Start>d__3>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<SettingsWindow.<SwitchLanguageAsync>d__11>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<ShopWindow.<DrawPackAsync>d__34>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<ShopWindow.<PurchaseAsync>d__33>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<ShopWindow.<RefreshCurrentCategoryAsync>d__30>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<SingletonManager.<Start>d__2>
	// Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<SocialPortrait.<ApplyAsync>d__0>
	// Cysharp.Threading.Tasks.CompilerServices.IStateMachineRunnerPromise<AChen.Networking.BackendHttpResponse>
	// Cysharp.Threading.Tasks.CompilerServices.IStateMachineRunnerPromise<AChen.Player.AuthResult>
	// Cysharp.Threading.Tasks.CompilerServices.IStateMachineRunnerPromise<CardPickViewData>
	// Cysharp.Threading.Tasks.CompilerServices.IStateMachineRunnerPromise<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// Cysharp.Threading.Tasks.CompilerServices.IStateMachineRunnerPromise<byte>
	// Cysharp.Threading.Tasks.CompilerServices.IStateMachineRunnerPromise<object>
	// Cysharp.Threading.Tasks.ITaskPoolNode<object>
	// Cysharp.Threading.Tasks.IUniTaskSource<AChen.Networking.BackendHttpResponse>
	// Cysharp.Threading.Tasks.IUniTaskSource<AChen.Player.AuthResult>
	// Cysharp.Threading.Tasks.IUniTaskSource<CardPickViewData>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,CardPickViewData>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,byte>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<object,object>>
	// Cysharp.Threading.Tasks.IUniTaskSource<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// Cysharp.Threading.Tasks.IUniTaskSource<byte>
	// Cysharp.Threading.Tasks.IUniTaskSource<object>
	// Cysharp.Threading.Tasks.Internal.StatePool<Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<object,object>>>
	// Cysharp.Threading.Tasks.Internal.StatePool<Cysharp.Threading.Tasks.UniTask.Awaiter<object>>
	// Cysharp.Threading.Tasks.Internal.StateTuple<Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<object,object>>>
	// Cysharp.Threading.Tasks.Internal.StateTuple<Cysharp.Threading.Tasks.UniTask.Awaiter<object>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<AChen.Networking.BackendHttpResponse>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<AChen.Player.AuthResult>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<CardPickViewData>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,CardPickViewData>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,byte>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<object,object>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<byte>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<object>
	// Cysharp.Threading.Tasks.UniTask.CanceledResultSource<System.ValueTuple<object,object>>
	// Cysharp.Threading.Tasks.UniTask.CanceledResultSource<object>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<AChen.Networking.BackendHttpResponse>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<AChen.Player.AuthResult>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<CardPickViewData>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,CardPickViewData>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,byte>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<object,object>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<byte>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<object>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<AChen.Networking.BackendHttpResponse>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<AChen.Player.AuthResult>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<CardPickViewData>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,CardPickViewData>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,byte>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<object,object>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<byte>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<object>
	// Cysharp.Threading.Tasks.UniTask.WhenAllPromise.<>c<CardPickViewData>
	// Cysharp.Threading.Tasks.UniTask.WhenAllPromise.<>c<object,object>
	// Cysharp.Threading.Tasks.UniTask.WhenAllPromise.<>c<object>
	// Cysharp.Threading.Tasks.UniTask.WhenAllPromise<CardPickViewData>
	// Cysharp.Threading.Tasks.UniTask.WhenAllPromise<object,object>
	// Cysharp.Threading.Tasks.UniTask.WhenAllPromise<object>
	// Cysharp.Threading.Tasks.UniTask<AChen.Networking.BackendHttpResponse>
	// Cysharp.Threading.Tasks.UniTask<AChen.Player.AuthResult>
	// Cysharp.Threading.Tasks.UniTask<CardPickViewData>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,CardPickViewData>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,byte>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<object,object>>
	// Cysharp.Threading.Tasks.UniTask<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// Cysharp.Threading.Tasks.UniTask<byte>
	// Cysharp.Threading.Tasks.UniTask<object>
	// Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<AChen.Networking.BackendHttpResponse>
	// Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<AChen.Player.AuthResult>
	// Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<CardPickViewData>
	// Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<Cysharp.Threading.Tasks.AsyncUnit>
	// Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<System.ValueTuple<object,object>>
	// Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<byte>
	// Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<object>
	// Cysharp.Threading.Tasks.UniTaskExtensions.<>c__51<System.ValueTuple<object,object>>
	// Cysharp.Threading.Tasks.UniTaskExtensions.<>c__51<object>
	// Cysharp.Threading.Tasks.UniTaskExtensions.AttachExternalCancellationSource<System.ValueTuple<object,object>>
	// Cysharp.Threading.Tasks.UniTaskExtensions.AttachExternalCancellationSource<object>
	// DelegateList<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// DelegateList<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<long>>
	// DelegateList<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// DelegateList<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// DelegateList<float>
	// LitMotion.Extensions.LitMotionRectTransformExtensions.<>c__0<LitMotion.NoOptions,LitMotion.Adapters.Vector2MotionAdapter>
	// LitMotion.Extensions.LitMotionTransformExtensions.<>c__30<LitMotion.NoOptions,LitMotion.Adapters.Vector3MotionAdapter>
	// LitMotion.Extensions.LitMotionUGUIExtensions.<>c__4<LitMotion.NoOptions,LitMotion.Adapters.FloatMotionAdapter>
	// LitMotion.Extensions.LitMotionUGUIExtensions.<>c__6<LitMotion.NoOptions,LitMotion.Adapters.FloatMotionAdapter>
	// LitMotion.MotionBuilder<UnityEngine.Vector2,LitMotion.NoOptions,LitMotion.Adapters.Vector2MotionAdapter>
	// LitMotion.MotionBuilder<UnityEngine.Vector3,LitMotion.NoOptions,LitMotion.Adapters.Vector3MotionAdapter>
	// LitMotion.MotionBuilder<float,LitMotion.NoOptions,LitMotion.Adapters.FloatMotionAdapter>
	// LitMotion.MotionBuilderBuffer<UnityEngine.Vector2,LitMotion.NoOptions>
	// LitMotion.MotionBuilderBuffer<UnityEngine.Vector3,LitMotion.NoOptions>
	// LitMotion.MotionBuilderBuffer<float,LitMotion.NoOptions>
	// LitMotion.MotionSettings<UnityEngine.Vector2,LitMotion.NoOptions>
	// LitMotion.MotionSettings<UnityEngine.Vector3,LitMotion.NoOptions>
	// LitMotion.MotionSettings<float,LitMotion.NoOptions>
	// System.Action<AChen.Networking.GiftRewardSpec>
	// System.Action<CardPickViewData>
	// System.Action<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Action<System.Nullable<System.Guid>,object>
	// System.Action<System.Nullable<int>>
	// System.Action<System.Nullable<long>>
	// System.Action<UnityEngine.EventSystems.RaycastResult>
	// System.Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,object>
	// System.Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<long>>
	// System.Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Action<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Action<UnityEngine.Vector2,object>
	// System.Action<UnityEngine.Vector3,object>
	// System.Action<byte>
	// System.Action<float,object>
	// System.Action<float>
	// System.Action<int,int>
	// System.Action<int>
	// System.Action<object,WindowOpenRequest>
	// System.Action<object,byte>
	// System.Action<object,int>
	// System.Action<object,object,object>
	// System.Action<object,object>
	// System.Action<object>
	// System.Collections.Concurrent.ConcurrentQueue.<Enumerate>d__28<object>
	// System.Collections.Concurrent.ConcurrentQueue.Segment<object>
	// System.Collections.Concurrent.ConcurrentQueue<object>
	// System.Collections.Generic.ArraySortHelper<AChen.Networking.GiftRewardSpec>
	// System.Collections.Generic.ArraySortHelper<CardPickViewData>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<AChen.Networking.BackendHttpResponse>
	// System.Collections.Generic.Comparer<AChen.Networking.GiftRewardSpec>
	// System.Collections.Generic.Comparer<AChen.Player.AuthResult>
	// System.Collections.Generic.Comparer<CardPickViewData>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,CardPickViewData>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,byte>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,object>>
	// System.Collections.Generic.Comparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.Comparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.Comparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.Comparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.Comparer<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Collections.Generic.Comparer<byte>
	// System.Collections.Generic.Comparer<int>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.ComparisonComparer<AChen.Networking.BackendHttpResponse>
	// System.Collections.Generic.ComparisonComparer<AChen.Networking.GiftRewardSpec>
	// System.Collections.Generic.ComparisonComparer<AChen.Player.AuthResult>
	// System.Collections.Generic.ComparisonComparer<CardPickViewData>
	// System.Collections.Generic.ComparisonComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,CardPickViewData>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,byte>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<byte,object>>
	// System.Collections.Generic.ComparisonComparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.ComparisonComparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ComparisonComparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.ComparisonComparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.ComparisonComparer<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Collections.Generic.ComparisonComparer<byte>
	// System.Collections.Generic.ComparisonComparer<int>
	// System.Collections.Generic.ComparisonComparer<object>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.Dictionary.Enumerator<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.Dictionary.KeyCollection<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.Dictionary.ValueCollection<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.Dictionary<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.EqualityComparer<AChen.Networking.BackendHttpResponse>
	// System.Collections.Generic.EqualityComparer<AChen.Player.AuthResult>
	// System.Collections.Generic.EqualityComparer<CardPickViewData>
	// System.Collections.Generic.EqualityComparer<LitMotion.NoOptions>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,CardPickViewData>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,byte>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,object>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.EqualityComparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.EqualityComparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.EqualityComparer<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Collections.Generic.EqualityComparer<UnityEngine.Vector2>
	// System.Collections.Generic.EqualityComparer<UnityEngine.Vector3>
	// System.Collections.Generic.EqualityComparer<byte>
	// System.Collections.Generic.EqualityComparer<float>
	// System.Collections.Generic.EqualityComparer<int>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.HashSet.Enumerator<int>
	// System.Collections.Generic.HashSet.Enumerator<object>
	// System.Collections.Generic.HashSet<int>
	// System.Collections.Generic.HashSet<object>
	// System.Collections.Generic.HashSetEqualityComparer<int>
	// System.Collections.Generic.HashSetEqualityComparer<object>
	// System.Collections.Generic.ICollection<AChen.Networking.GiftRewardSpec>
	// System.Collections.Generic.ICollection<CardPickViewData>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ICollection<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.ICollection<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.ICollection<int>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<AChen.Networking.GiftRewardSpec>
	// System.Collections.Generic.IComparer<CardPickViewData>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IComparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IComparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.IComparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.IComparer<int>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IEnumerable<AChen.Networking.GiftRewardSpec>
	// System.Collections.Generic.IEnumerable<CardPickViewData>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IEnumerable<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.IEnumerable<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.IEnumerable<int>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerable<ushort>
	// System.Collections.Generic.IEnumerator<AChen.Networking.GiftRewardSpec>
	// System.Collections.Generic.IEnumerator<CardPickViewData>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IEnumerator<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.IEnumerator<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.IEnumerator<int>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEnumerator<ushort>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IList<AChen.Networking.GiftRewardSpec>
	// System.Collections.Generic.IList<CardPickViewData>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IList<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IList<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.IList<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.IReadOnlyCollection<CardDetailEntry>
	// System.Collections.Generic.IReadOnlyCollection<CardPickViewData>
	// System.Collections.Generic.IReadOnlyCollection<object>
	// System.Collections.Generic.IReadOnlyDictionary<int,object>
	// System.Collections.Generic.IReadOnlyList<CardDetailEntry>
	// System.Collections.Generic.IReadOnlyList<CardPickViewData>
	// System.Collections.Generic.IReadOnlyList<object>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.KeyValuePair<object,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.LinkedList.Enumerator<object>
	// System.Collections.Generic.LinkedList<object>
	// System.Collections.Generic.LinkedListNode<object>
	// System.Collections.Generic.List.Enumerator<AChen.Networking.GiftRewardSpec>
	// System.Collections.Generic.List.Enumerator<CardPickViewData>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.List.Enumerator<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.List.Enumerator<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.List.Enumerator<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<AChen.Networking.GiftRewardSpec>
	// System.Collections.Generic.List<CardPickViewData>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.List<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.List<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<AChen.Networking.BackendHttpResponse>
	// System.Collections.Generic.ObjectComparer<AChen.Networking.GiftRewardSpec>
	// System.Collections.Generic.ObjectComparer<AChen.Player.AuthResult>
	// System.Collections.Generic.ObjectComparer<CardPickViewData>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,CardPickViewData>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,byte>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,object>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.ObjectComparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ObjectComparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.ObjectComparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.Generic.ObjectComparer<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Collections.Generic.ObjectComparer<byte>
	// System.Collections.Generic.ObjectComparer<int>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<AChen.Networking.BackendHttpResponse>
	// System.Collections.Generic.ObjectEqualityComparer<AChen.Player.AuthResult>
	// System.Collections.Generic.ObjectEqualityComparer<CardPickViewData>
	// System.Collections.Generic.ObjectEqualityComparer<LitMotion.NoOptions>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,CardPickViewData>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,byte>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,object>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.Vector2>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.Vector3>
	// System.Collections.Generic.ObjectEqualityComparer<byte>
	// System.Collections.Generic.ObjectEqualityComparer<float>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.Queue.Enumerator<WindowHistoryEntry>
	// System.Collections.Generic.Queue<WindowHistoryEntry>
	// System.Collections.Generic.Stack.Enumerator<WindowHistoryEntry>
	// System.Collections.Generic.Stack.Enumerator<object>
	// System.Collections.Generic.Stack<WindowHistoryEntry>
	// System.Collections.Generic.Stack<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<AChen.Networking.GiftRewardSpec>
	// System.Collections.ObjectModel.ReadOnlyCollection<CardPickViewData>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<AChen.Networking.BackendHttpResponse>
	// System.Comparison<AChen.Networking.GiftRewardSpec>
	// System.Comparison<AChen.Player.AuthResult>
	// System.Comparison<CardPickViewData>
	// System.Comparison<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Comparison<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Comparison<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Comparison<System.ValueTuple<byte,CardPickViewData>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Comparison<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Comparison<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Comparison<System.ValueTuple<byte,byte>>
	// System.Comparison<System.ValueTuple<byte,object>>
	// System.Comparison<System.ValueTuple<object,object>>
	// System.Comparison<UnityEngine.EventSystems.RaycastResult>
	// System.Comparison<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Comparison<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Comparison<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Comparison<byte>
	// System.Comparison<int>
	// System.Comparison<object>
	// System.Func<AChen.Networking.BackendHttpResponse>
	// System.Func<AChen.Player.AuthResult>
	// System.Func<CardPickViewData>
	// System.Func<System.Collections.Generic.KeyValuePair<object,object>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<object,object>,object>
	// System.Func<System.Threading.CancellationToken,Cysharp.Threading.Tasks.UniTask>
	// System.Func<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Func<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Func<System.ValueTuple<byte,CardPickViewData>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Func<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Func<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Func<System.ValueTuple<byte,byte>>
	// System.Func<System.ValueTuple<byte,object>>
	// System.Func<System.ValueTuple<object,object>>
	// System.Func<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Func<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Func<byte>
	// System.Func<int,byte>
	// System.Func<int,int>
	// System.Func<int>
	// System.Func<long,byte>
	// System.Func<long>
	// System.Func<object,AChen.Networking.BackendHttpResponse>
	// System.Func<object,AChen.Player.AuthResult>
	// System.Func<object,CardPickViewData>
	// System.Func<object,System.Threading.CancellationToken,Cysharp.Threading.Tasks.UniTask<byte>>
	// System.Func<object,System.Threading.CancellationToken,Cysharp.Threading.Tasks.UniTask<object>>
	// System.Func<object,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Func<object,System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Func<object,System.ValueTuple<byte,CardPickViewData>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Func<object,System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Func<object,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Func<object,System.ValueTuple<byte,byte>>
	// System.Func<object,System.ValueTuple<byte,object>>
	// System.Func<object,System.ValueTuple<object,object>>
	// System.Func<object,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Func<object,byte>
	// System.Func<object,int,object>
	// System.Func<object,int>
	// System.Func<object,long>
	// System.Func<object,object,object>
	// System.Func<object,object>
	// System.Func<object>
	// System.Func<ushort,byte>
	// System.IProgress<float>
	// System.Linq.Buffer<int>
	// System.Linq.Buffer<object>
	// System.Linq.Enumerable.Iterator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Linq.Enumerable.Iterator<int>
	// System.Linq.Enumerable.Iterator<object>
	// System.Linq.Enumerable.WhereArrayIterator<object>
	// System.Linq.Enumerable.WhereEnumerableIterator<int>
	// System.Linq.Enumerable.WhereEnumerableIterator<object>
	// System.Linq.Enumerable.WhereListIterator<object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<System.Collections.Generic.KeyValuePair<object,object>,object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,int>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,object>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<System.Collections.Generic.KeyValuePair<object,object>,object>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,int>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,object>
	// System.Linq.Enumerable.WhereSelectListIterator<System.Collections.Generic.KeyValuePair<object,object>,object>
	// System.Linq.Enumerable.WhereSelectListIterator<object,int>
	// System.Linq.Enumerable.WhereSelectListIterator<object,object>
	// System.Linq.EnumerableSorter<int,int>
	// System.Linq.EnumerableSorter<int>
	// System.Linq.EnumerableSorter<object,int>
	// System.Linq.EnumerableSorter<object,object>
	// System.Linq.EnumerableSorter<object>
	// System.Linq.IOrderedEnumerable<object>
	// System.Linq.OrderedEnumerable.<GetEnumerator>d__1<int>
	// System.Linq.OrderedEnumerable.<GetEnumerator>d__1<object>
	// System.Linq.OrderedEnumerable<int,int>
	// System.Linq.OrderedEnumerable<int>
	// System.Linq.OrderedEnumerable<object,int>
	// System.Linq.OrderedEnumerable<object,object>
	// System.Linq.OrderedEnumerable<object>
	// System.Nullable<AChen.Events.EventId<object,object>>
	// System.Nullable<System.DateTimeOffset>
	// System.Nullable<System.Guid>
	// System.Nullable<int>
	// System.Nullable<long>
	// System.Predicate<AChen.Networking.GiftRewardSpec>
	// System.Predicate<CardPickViewData>
	// System.Predicate<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Predicate<UnityEngine.EventSystems.RaycastResult>
	// System.Predicate<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>
	// System.Predicate<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle>
	// System.Predicate<int>
	// System.Predicate<object>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<AChen.Networking.BackendHttpResponse>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<AChen.Player.AuthResult>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<CardPickViewData>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,CardPickViewData>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,byte>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<byte,object>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.ValueTuple<object,object>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<byte>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<AChen.Networking.BackendHttpResponse>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<AChen.Player.AuthResult>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<CardPickViewData>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,CardPickViewData>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,byte>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,object>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<object,object>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<byte>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<long>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<AChen.Networking.BackendHttpResponse>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<AChen.Player.AuthResult>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<CardPickViewData>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,CardPickViewData>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,byte>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,object>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<object,object>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<byte>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<long>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<AChen.Networking.BackendHttpResponse>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<AChen.Player.AuthResult>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<CardPickViewData>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,CardPickViewData>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,byte>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,object>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<object,object>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<byte>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<AChen.Networking.BackendHttpResponse>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<AChen.Player.AuthResult>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<CardPickViewData>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,CardPickViewData>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,byte>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,object>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<object,object>>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<byte>
	// System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<object>
	// System.Runtime.CompilerServices.TaskAwaiter<AChen.Networking.BackendHttpResponse>
	// System.Runtime.CompilerServices.TaskAwaiter<AChen.Player.AuthResult>
	// System.Runtime.CompilerServices.TaskAwaiter<CardPickViewData>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,CardPickViewData>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,byte>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,object>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<object,object>>
	// System.Runtime.CompilerServices.TaskAwaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Runtime.CompilerServices.TaskAwaiter<byte>
	// System.Runtime.CompilerServices.TaskAwaiter<long>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<AChen.Networking.BackendHttpResponse>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<AChen.Player.AuthResult>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<CardPickViewData>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,CardPickViewData>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,byte>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,object>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<object,object>>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<byte>
	// System.Runtime.CompilerServices.ValueTaskAwaiter<object>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<AChen.Networking.BackendHttpResponse>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<AChen.Player.AuthResult>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<CardPickViewData>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,CardPickViewData>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,byte>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,object>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<object,object>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<byte>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<long>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<object>
	// System.Threading.Tasks.Sources.IValueTaskSource<AChen.Networking.BackendHttpResponse>
	// System.Threading.Tasks.Sources.IValueTaskSource<AChen.Player.AuthResult>
	// System.Threading.Tasks.Sources.IValueTaskSource<CardPickViewData>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,CardPickViewData>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,byte>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,object>>
	// System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<object,object>>
	// System.Threading.Tasks.Sources.IValueTaskSource<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Threading.Tasks.Sources.IValueTaskSource<byte>
	// System.Threading.Tasks.Sources.IValueTaskSource<object>
	// System.Threading.Tasks.Task<AChen.Networking.BackendHttpResponse>
	// System.Threading.Tasks.Task<AChen.Player.AuthResult>
	// System.Threading.Tasks.Task<CardPickViewData>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,CardPickViewData>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,byte>>
	// System.Threading.Tasks.Task<System.ValueTuple<byte,object>>
	// System.Threading.Tasks.Task<System.ValueTuple<object,object>>
	// System.Threading.Tasks.Task<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Threading.Tasks.Task<byte>
	// System.Threading.Tasks.Task<long>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.TaskCompletionSource<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Threading.Tasks.TaskCompletionSource<long>
	// System.Threading.Tasks.TaskCompletionSource<object>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<AChen.Networking.BackendHttpResponse>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<AChen.Player.AuthResult>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<CardPickViewData>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,CardPickViewData>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,byte>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<byte,object>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.ValueTuple<object,object>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<byte>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<long>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<object>
	// System.Threading.Tasks.TaskFactory<AChen.Networking.BackendHttpResponse>
	// System.Threading.Tasks.TaskFactory<AChen.Player.AuthResult>
	// System.Threading.Tasks.TaskFactory<CardPickViewData>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,CardPickViewData>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,byte>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,object>>
	// System.Threading.Tasks.TaskFactory<System.ValueTuple<object,object>>
	// System.Threading.Tasks.TaskFactory<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Threading.Tasks.TaskFactory<byte>
	// System.Threading.Tasks.TaskFactory<long>
	// System.Threading.Tasks.TaskFactory<object>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<AChen.Networking.BackendHttpResponse>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<AChen.Player.AuthResult>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<CardPickViewData>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,CardPickViewData>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,byte>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<byte,object>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<System.ValueTuple<object,object>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<byte>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c<object>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<AChen.Networking.BackendHttpResponse>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<AChen.Player.AuthResult>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<CardPickViewData>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,CardPickViewData>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,byte>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<byte,object>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<System.ValueTuple<object,object>>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<byte>
	// System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask<object>
	// System.Threading.Tasks.ValueTask<AChen.Networking.BackendHttpResponse>
	// System.Threading.Tasks.ValueTask<AChen.Player.AuthResult>
	// System.Threading.Tasks.ValueTask<CardPickViewData>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,CardPickViewData>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,byte>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<byte,object>>
	// System.Threading.Tasks.ValueTask<System.ValueTuple<object,object>>
	// System.Threading.Tasks.ValueTask<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.Threading.Tasks.ValueTask<byte>
	// System.Threading.Tasks.ValueTask<object>
	// System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>
	// System.ValueTuple<byte,AChen.Player.AuthResult>
	// System.ValueTuple<byte,CardPickViewData>
	// System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>
	// System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>
	// System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Networking.BackendHttpResponse>>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AChen.Player.AuthResult>>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,CardPickViewData>>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<object,object>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>
	// System.ValueTuple<byte,System.ValueTuple<byte,byte>>
	// System.ValueTuple<byte,System.ValueTuple<byte,object>>
	// System.ValueTuple<byte,System.ValueTuple<object,object>>
	// System.ValueTuple<byte,UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// System.ValueTuple<byte,byte>
	// System.ValueTuple<byte,object>
	// System.ValueTuple<object,object>
	// UnityEngine.AddressableAssets.AddressablesImpl.<>c__DisplayClass78_0<object>
	// UnityEngine.AddressableAssets.AddressablesImpl.<>c__DisplayClass79_0<object>
	// UnityEngine.AddressableAssets.AssetReferenceT<object>
	// UnityEngine.Events.InvokableCall<object>
	// UnityEngine.Events.UnityAction<object>
	// UnityEngine.Events.UnityEvent<object>
	// UnityEngine.InputSystem.InputControl<UnityEngine.Vector2>
	// UnityEngine.InputSystem.InputProcessor<UnityEngine.Vector2>
	// UnityEngine.InputSystem.Utilities.InlinedArray<object>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase.<>c__DisplayClass60_0<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase.<>c__DisplayClass60_0<long>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase.<>c__DisplayClass60_0<object>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase.<>c__DisplayClass61_0<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase.<>c__DisplayClass61_0<long>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase.<>c__DisplayClass61_0<object>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase<long>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase<object>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.<>c<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.<>c<long>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.<>c<object>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<long>
	// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>
	// UnityEngine.ResourceManagement.ChainOperationTypelessDepedency<object>
	// UnityEngine.ResourceManagement.ResourceManager.CompletedOperation<object>
	// UnityEngine.ResourceManagement.Util.GlobalLinkedListNodeCache<object>
	// UnityEngine.ResourceManagement.Util.LinkedListNodeCache<object>
	// }}

	public void RefMethods()
	{
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,AChen.Networking.ContentUpdatePrompt.<WaitForRetryAsync>d__10>(Cysharp.Threading.Tasks.UniTask.Awaiter&,AChen.Networking.ContentUpdatePrompt.<WaitForRetryAsync>d__10&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,AChen.Networking.LocalGameConfiguration.<CheckVersionAsync>d__9>(Cysharp.Threading.Tasks.UniTask.Awaiter&,AChen.Networking.LocalGameConfiguration.<CheckVersionAsync>d__9&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,AChen.Player.PlayerSession.<LogoutAsync>d__28>(Cysharp.Threading.Tasks.UniTask.Awaiter&,AChen.Player.PlayerSession.<LogoutAsync>d__28&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,AUIScreenController.<RunTransitionAsync>d__70>(Cysharp.Threading.Tasks.UniTask.Awaiter&,AUIScreenController.<RunTransitionAsync>d__70&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,AvatarSelectWindow.<BindListAsync>d__24>(Cysharp.Threading.Tasks.UniTask.Awaiter&,AvatarSelectWindow.<BindListAsync>d__24&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,CardPackShopCategory.<BindAsync>d__3>(Cysharp.Threading.Tasks.UniTask.Awaiter&,CardPackShopCategory.<BindAsync>d__3&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,CardPickController.<DoEndShow>d__30>(Cysharp.Threading.Tasks.UniTask.Awaiter&,CardPickController.<DoEndShow>d__30&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,CardZoomWindow.<PlayCloseBowAsync>d__30>(Cysharp.Threading.Tasks.UniTask.Awaiter&,CardZoomWindow.<PlayCloseBowAsync>d__30&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,CardZoomWindow.<PlayOpenBowAsync>d__29>(Cysharp.Threading.Tasks.UniTask.Awaiter&,CardZoomWindow.<PlayOpenBowAsync>d__29&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,CosmeticShopCategory.<BindAsync>d__9>(Cysharp.Threading.Tasks.UniTask.Awaiter&,CosmeticShopCategory.<BindAsync>d__9&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,FriendWindow.<<LoadFriendsAsync>b__14_0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter&,FriendWindow.<<LoadFriendsAsync>b__14_0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,FriendWindow.<>c__DisplayClass15_0.<<SearchAsync>b__0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter&,FriendWindow.<>c__DisplayClass15_0.<<SearchAsync>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,FriendWindow.<>c__DisplayClass16_0.<<SendRequestAsync>b__0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter&,FriendWindow.<>c__DisplayClass16_0.<<SendRequestAsync>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,FriendWindow.<BindRowsAsync>d__17>(Cysharp.Threading.Tasks.UniTask.Awaiter&,FriendWindow.<BindRowsAsync>d__17&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,GameFlow.<CheckContentAsync>d__4>(Cysharp.Threading.Tasks.UniTask.Awaiter&,GameFlow.<CheckContentAsync>d__4&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,GiftWindow.<<LoadInboxAsync>b__9_0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter&,GiftWindow.<<LoadInboxAsync>b__9_0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,GiftWindow.<>c__DisplayClass10_0.<<AcceptAsync>b__0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter&,GiftWindow.<>c__DisplayClass10_0.<<AcceptAsync>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,GiftWindow.<>c__DisplayClass11_0.<<RejectAsync>b__0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter&,GiftWindow.<>c__DisplayClass11_0.<<RejectAsync>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,GiftWindow.<>c__DisplayClass12_0.<<ClaimAsync>b__0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter&,GiftWindow.<>c__DisplayClass12_0.<<ClaimAsync>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,GiftWindow.<RefreshInboxAsync>d__13>(Cysharp.Threading.Tasks.UniTask.Awaiter&,GiftWindow.<RefreshInboxAsync>d__13&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,LobbyWallpaperView.<WaitForReadyAsync>d__10>(Cysharp.Threading.Tasks.UniTask.Awaiter&,LobbyWallpaperView.<WaitForReadyAsync>d__10&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,LogInWindow.<DoAnim>d__32>(Cysharp.Threading.Tasks.UniTask.Awaiter&,LogInWindow.<DoAnim>d__32&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,PreGameUIPanel.<<SwitchToNextWallpaperAsync>b__38_0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter&,PreGameUIPanel.<<SwitchToNextWallpaperAsync>b__38_0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,PreGameUIPanel.<PlayIntroAsync>d__39>(Cysharp.Threading.Tasks.UniTask.Awaiter&,PreGameUIPanel.<PlayIntroAsync>d__39&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<object,object>>,LobbyWallpaperView.<LoadAsync>d__16>(Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<object,object>>&,LobbyWallpaperView.<LoadAsync>d__16&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Player.PlayerSession.<RefreshAsync>d__27>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Player.PlayerSession.<RefreshAsync>d__27&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AvatarSelectWindow.<BindListAsync>d__24>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AvatarSelectWindow.<BindListAsync>d__24&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,CardPackShopCategory.<BindAsync>d__3>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,CardPackShopCategory.<BindAsync>d__3&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,CosmeticShopCategory.<BindAsync>d__9>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,CosmeticShopCategory.<BindAsync>d__9&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,FriendWindow.<<LoadFriendsAsync>b__14_0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,FriendWindow.<<LoadFriendsAsync>b__14_0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,FriendWindow.<>c__DisplayClass15_0.<<SearchAsync>b__0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,FriendWindow.<>c__DisplayClass15_0.<<SearchAsync>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,GiftWindow.<<LoadInboxAsync>b__9_0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,GiftWindow.<<LoadInboxAsync>b__9_0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,GiftWindow.<>c__DisplayClass12_0.<<ClaimAsync>b__0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,GiftWindow.<>c__DisplayClass12_0.<<ClaimAsync>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,GiftWindow.<RefreshInboxAsync>d__13>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,GiftWindow.<RefreshInboxAsync>d__13&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,InboxListController.<BindAsync>d__6>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,InboxListController.<BindAsync>d__6&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,PlayerProfileView.<LoadAvatarAsync>d__9>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,PlayerProfileView.<LoadAvatarAsync>d__9&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,PreGameUIPanel.<<SwitchToNextWallpaperAsync>b__38_0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,PreGameUIPanel.<<SwitchToNextWallpaperAsync>b__38_0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,UpdateDetector.<DownloadAssets>d__5>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,UpdateDetector.<DownloadAssets>d__5&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<LitMotion.MotionAwaiter,CardZoomWindow.<AnimateBowAsync>d__31>(LitMotion.MotionAwaiter&,CardZoomWindow.<AnimateBowAsync>d__31&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<LitMotion.MotionAwaiter,LobbyWallpaperView.<PlayRevealAsync>d__15>(LitMotion.MotionAwaiter&,LobbyWallpaperView.<PlayRevealAsync>d__15&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<LitMotion.MotionAwaiter,UITween.<RunAsync>d__7>(LitMotion.MotionAwaiter&,UITween.<RunAsync>d__7&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>,AddressableLoader.<UnloadScene>d__26>(System.Runtime.CompilerServices.TaskAwaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>&,AddressableLoader.<UnloadScene>d__26&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<long>,UpdateDetector.<DownloadAssets>d__5>(System.Runtime.CompilerServices.TaskAwaiter<long>&,UpdateDetector.<DownloadAssets>d__5&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,AChen.Networking.LocalGameConfiguration.<InitializeAsync>d__8>(System.Runtime.CompilerServices.TaskAwaiter<object>&,AChen.Networking.LocalGameConfiguration.<InitializeAsync>d__8&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,UpdateDetector.<DownloadAssets>d__5>(System.Runtime.CompilerServices.TaskAwaiter<object>&,UpdateDetector.<DownloadAssets>d__5&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,UpdateDetector.<InitializeLocalAsync>d__4>(System.Runtime.CompilerServices.TaskAwaiter<object>&,UpdateDetector.<InitializeLocalAsync>d__4&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<AChen.Networking.BackendHttpResponse>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Networking.BackendHttpClient.<SendRawAsync>d__6>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Networking.BackendHttpClient.<SendRawAsync>d__6&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<AChen.Player.AuthResult>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Player.AuthFlow.<AuthenticateAsync>d__3>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Player.AuthFlow.<AuthenticateAsync>d__3&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<CardPickViewData>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,ShopWindow.<LoadDrawCardAsync>d__36>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,ShopWindow.<LoadDrawCardAsync>d__36&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,SceneLoader.<LoadScene>d__1>(Cysharp.Threading.Tasks.UniTask.Awaiter&,SceneLoader.<LoadScene>d__1&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,SceneLoader.<ReloadScene>d__2>(Cysharp.Threading.Tasks.UniTask.Awaiter&,SceneLoader.<ReloadScene>d__2&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>,AddressableLoader.<LoadScene>d__21>(Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>&,AddressableLoader.<LoadScene>d__21&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>,AddressableLoader.<ReloadScene>d__22>(Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>&,AddressableLoader.<ReloadScene>d__22&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>,SceneLoader.<LoadScene>d__1>(Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>&,SceneLoader.<LoadScene>d__1&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>,SceneLoader.<ReloadScene>d__2>(Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>&,SceneLoader.<ReloadScene>d__2&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,AddressableLoader.<AwaitScene>d__29>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,AddressableLoader.<AwaitScene>d__29&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>,AddressableLoader.<AwaitScene>d__29>(System.Runtime.CompilerServices.TaskAwaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>&,AddressableLoader.<AwaitScene>d__29&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<byte>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,AChen.Player.PlayerSession.<>c__DisplayClass38_0.<<SendFriendRequestAsync>b__0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter&,AChen.Player.PlayerSession.<>c__DisplayClass38_0.<<SendFriendRequestAsync>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<byte>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,AChen.Player.PlayerSession.<>c__DisplayClass39_0.<<AcceptFriendRequestAsync>b__0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter&,AChen.Player.PlayerSession.<>c__DisplayClass39_0.<<AcceptFriendRequestAsync>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<byte>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,AChen.Player.PlayerSession.<>c__DisplayClass40_0.<<RejectFriendRequestAsync>b__0>d>(Cysharp.Threading.Tasks.UniTask.Awaiter&,AChen.Player.PlayerSession.<>c__DisplayClass40_0.<<RejectFriendRequestAsync>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<byte>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,AChen.Player.PlayerSession.<TryRestoreSessionAsync>d__26>(Cysharp.Threading.Tasks.UniTask.Awaiter&,AChen.Player.PlayerSession.<TryRestoreSessionAsync>d__26&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<byte>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,AUIScreenController.<RunGuardedAsync>d__46>(Cysharp.Threading.Tasks.UniTask.Awaiter&,AUIScreenController.<RunGuardedAsync>d__46&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,AChen.Player.PlayerSession.<SendAuthenticatedAsync>d__49>(Cysharp.Threading.Tasks.UniTask.Awaiter&,AChen.Player.PlayerSession.<SendAuthenticatedAsync>d__49&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,AChen.Player.PlayerSession.<SendAuthenticatedCallAsync>d__48<object>>(Cysharp.Threading.Tasks.UniTask.Awaiter&,AChen.Player.PlayerSession.<SendAuthenticatedCallAsync>d__48<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,AChen.Player.PlayerSession.<SendAuthenticatedDrawAsync>d__51>(Cysharp.Threading.Tasks.UniTask.Awaiter&,AChen.Player.PlayerSession.<SendAuthenticatedDrawAsync>d__51&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,AChen.Player.PlayerSession.<SendAuthenticatedPoolAsync>d__50>(Cysharp.Threading.Tasks.UniTask.Awaiter&,AChen.Player.PlayerSession.<SendAuthenticatedPoolAsync>d__50&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,GameFlow.<GetStartupSceneAsync>d__10>(Cysharp.Threading.Tasks.UniTask.Awaiter&,GameFlow.<GetStartupSceneAsync>d__10&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<AChen.Networking.BackendHttpResponse>,AChen.Networking.BackendHttpClient.<SendAsync>d__4>(Cysharp.Threading.Tasks.UniTask.Awaiter<AChen.Networking.BackendHttpResponse>&,AChen.Networking.BackendHttpClient.<SendAsync>d__4&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<byte>,GameFlow.<GetStartupSceneAsync>d__10>(Cysharp.Threading.Tasks.UniTask.Awaiter<byte>&,GameFlow.<GetStartupSceneAsync>d__10&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Networking.AuthApi.<ClaimGiftAsync>d__23>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Networking.AuthApi.<ClaimGiftAsync>d__23&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Networking.AuthApi.<DrawCardsAsync>d__9>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Networking.AuthApi.<DrawCardsAsync>d__9&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Networking.AuthApi.<GetFriendsAsync>d__17>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Networking.AuthApi.<GetFriendsAsync>d__17&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Networking.AuthApi.<GetGachaPoolAsync>d__10>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Networking.AuthApi.<GetGachaPoolAsync>d__10&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Networking.AuthApi.<GetInboxAsync>d__22>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Networking.AuthApi.<GetInboxAsync>d__22&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Networking.AuthApi.<GetPlayerAsync>d__6>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Networking.AuthApi.<GetPlayerAsync>d__6&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Networking.AuthApi.<PostAuthAsync>d__24>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Networking.AuthApi.<PostAuthAsync>d__24&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Networking.AuthApi.<PurchaseShopItemAsync>d__8>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Networking.AuthApi.<PurchaseShopItemAsync>d__8&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Networking.AuthApi.<SearchFriendsAsync>d__18>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Networking.AuthApi.<SearchFriendsAsync>d__18&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Networking.AuthApi.<UpdateProfileAsync>d__7>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Networking.AuthApi.<UpdateProfileAsync>d__7&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Networking.BackendHttpClient.<SendAsync>d__5<object>>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Networking.BackendHttpClient.<SendAsync>d__5<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Player.PlayerSession.<ExecuteLockedAsync>d__47<object>>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Player.PlayerSession.<ExecuteLockedAsync>d__47<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Player.PlayerSession.<GetGachaPoolAsync>d__43>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Player.PlayerSession.<GetGachaPoolAsync>d__43&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Player.PlayerSession.<LoginAsync>d__25>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Player.PlayerSession.<LoginAsync>d__25&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Player.PlayerSession.<RegisterAsync>d__24>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Player.PlayerSession.<RegisterAsync>d__24&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Player.PlayerSession.<SendAuthenticatedAsync>d__49>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Player.PlayerSession.<SendAuthenticatedAsync>d__49&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Player.PlayerSession.<SendAuthenticatedCallAsync>d__48<object>>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Player.PlayerSession.<SendAuthenticatedCallAsync>d__48<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Player.PlayerSession.<SendAuthenticatedDrawAsync>d__51>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Player.PlayerSession.<SendAuthenticatedDrawAsync>d__51&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,AChen.Player.PlayerSession.<SendAuthenticatedPoolAsync>d__50>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,AChen.Player.PlayerSession.<SendAuthenticatedPoolAsync>d__50&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,CardPoolAddress.<LoadAddressAsync>d__7>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,CardPoolAddress.<LoadAddressAsync>d__7&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,CardPoolAddress.<LoadCardTextureAsync>d__6>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,CardPoolAddress.<LoadCardTextureAsync>d__6&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,CardPreviewWindow.<LoadCardImageAsync>d__16>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,CardPreviewWindow.<LoadCardImageAsync>d__16&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,CardPreviewWindow.<LoadCardImagesAsync>d__15>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,CardPreviewWindow.<LoadCardImagesAsync>d__15&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,GridListController.<LoadRowPrefabAsync>d__13>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,GridListController.<LoadRowPrefabAsync>d__13&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,ShopCatalogQuery.<LoadCardPacksAsync>d__0>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,ShopCatalogQuery.<LoadCardPacksAsync>d__0&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,ShopCatalogQuery.<LoadCosmeticsAsync>d__2<object>>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,ShopCatalogQuery.<LoadCosmeticsAsync>d__2<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,ShopCatalogQuery.<LoadSpriteAsync>d__6>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,ShopCatalogQuery.<LoadSpriteAsync>d__6&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,ShopCatalogQuery.<LoadSpritesAsync>d__5>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,ShopCatalogQuery.<LoadSpritesAsync>d__5&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,ShopWindow.<LoadDrawCardsAsync>d__35>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,ShopWindow.<LoadDrawCardsAsync>d__35&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,AChen.Player.PlayerSession.<ExecuteLockedAsync>d__47<object>>(System.Runtime.CompilerServices.TaskAwaiter&,AChen.Player.PlayerSession.<ExecuteLockedAsync>d__47<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,AddressableLoader.<LoadAsset>d__28<object,object>>(System.Runtime.CompilerServices.TaskAwaiter<object>&,AddressableLoader.<LoadAsset>d__28<object,object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,CardPoolAddress.<TryLoadAddressAsync>d__8>(System.Runtime.CompilerServices.TaskAwaiter<object>&,CardPoolAddress.<TryLoadAddressAsync>d__8&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<AChen.Networking.ContentUpdatePrompt.<WaitForRetryAsync>d__10>(AChen.Networking.ContentUpdatePrompt.<WaitForRetryAsync>d__10&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<AChen.Networking.LocalGameConfiguration.<CheckVersionAsync>d__9>(AChen.Networking.LocalGameConfiguration.<CheckVersionAsync>d__9&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<AChen.Networking.LocalGameConfiguration.<InitializeAsync>d__8>(AChen.Networking.LocalGameConfiguration.<InitializeAsync>d__8&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<AChen.Player.PlayerSession.<LogoutAsync>d__28>(AChen.Player.PlayerSession.<LogoutAsync>d__28&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<AChen.Player.PlayerSession.<RefreshAsync>d__27>(AChen.Player.PlayerSession.<RefreshAsync>d__27&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<AUIScreenController.<RunTransitionAsync>d__70>(AUIScreenController.<RunTransitionAsync>d__70&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<AddressableLoader.<UnloadScene>d__26>(AddressableLoader.<UnloadScene>d__26&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<AvatarSelectWindow.<BindListAsync>d__24>(AvatarSelectWindow.<BindListAsync>d__24&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<CardPackShopCategory.<BindAsync>d__3>(CardPackShopCategory.<BindAsync>d__3&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<CardPickController.<DoEndShow>d__30>(CardPickController.<DoEndShow>d__30&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<CardZoomWindow.<AnimateBowAsync>d__31>(CardZoomWindow.<AnimateBowAsync>d__31&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<CardZoomWindow.<PlayCloseBowAsync>d__30>(CardZoomWindow.<PlayCloseBowAsync>d__30&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<CardZoomWindow.<PlayOpenBowAsync>d__29>(CardZoomWindow.<PlayOpenBowAsync>d__29&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<CosmeticShopCategory.<BindAsync>d__9>(CosmeticShopCategory.<BindAsync>d__9&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<Cysharp.Threading.Tasks.UniTaskExtensions.<ContinueWith>d__54<object>>(Cysharp.Threading.Tasks.UniTaskExtensions.<ContinueWith>d__54<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<FriendWindow.<<LoadFriendsAsync>b__14_0>d>(FriendWindow.<<LoadFriendsAsync>b__14_0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<FriendWindow.<>c__DisplayClass15_0.<<SearchAsync>b__0>d>(FriendWindow.<>c__DisplayClass15_0.<<SearchAsync>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<FriendWindow.<>c__DisplayClass16_0.<<SendRequestAsync>b__0>d>(FriendWindow.<>c__DisplayClass16_0.<<SendRequestAsync>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<FriendWindow.<BindRowsAsync>d__17>(FriendWindow.<BindRowsAsync>d__17&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<GameFlow.<CheckContentAsync>d__4>(GameFlow.<CheckContentAsync>d__4&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<GiftWindow.<<LoadInboxAsync>b__9_0>d>(GiftWindow.<<LoadInboxAsync>b__9_0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<GiftWindow.<>c__DisplayClass10_0.<<AcceptAsync>b__0>d>(GiftWindow.<>c__DisplayClass10_0.<<AcceptAsync>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<GiftWindow.<>c__DisplayClass11_0.<<RejectAsync>b__0>d>(GiftWindow.<>c__DisplayClass11_0.<<RejectAsync>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<GiftWindow.<>c__DisplayClass12_0.<<ClaimAsync>b__0>d>(GiftWindow.<>c__DisplayClass12_0.<<ClaimAsync>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<GiftWindow.<RefreshInboxAsync>d__13>(GiftWindow.<RefreshInboxAsync>d__13&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<InboxListController.<BindAsync>d__6>(InboxListController.<BindAsync>d__6&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<LobbyWallpaperView.<LoadAsync>d__16>(LobbyWallpaperView.<LoadAsync>d__16&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<LobbyWallpaperView.<PlayRevealAsync>d__15>(LobbyWallpaperView.<PlayRevealAsync>d__15&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<LobbyWallpaperView.<WaitForReadyAsync>d__10>(LobbyWallpaperView.<WaitForReadyAsync>d__10&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<LogInWindow.<DoAnim>d__32>(LogInWindow.<DoAnim>d__32&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<PlayerProfileView.<LoadAvatarAsync>d__9>(PlayerProfileView.<LoadAvatarAsync>d__9&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<PreGameUIPanel.<<SwitchToNextWallpaperAsync>b__38_0>d>(PreGameUIPanel.<<SwitchToNextWallpaperAsync>b__38_0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<PreGameUIPanel.<PlayIntroAsync>d__39>(PreGameUIPanel.<PlayIntroAsync>d__39&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<UITween.<RunAsync>d__7>(UITween.<RunAsync>d__7&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<UpdateDetector.<DownloadAssets>d__5>(UpdateDetector.<DownloadAssets>d__5&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<UpdateDetector.<InitializeLocalAsync>d__4>(UpdateDetector.<InitializeLocalAsync>d__4&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<AChen.Networking.BackendHttpResponse>.Start<AChen.Networking.BackendHttpClient.<SendRawAsync>d__6>(AChen.Networking.BackendHttpClient.<SendRawAsync>d__6&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<AChen.Player.AuthResult>.Start<AChen.Player.AuthFlow.<AuthenticateAsync>d__3>(AChen.Player.AuthFlow.<AuthenticateAsync>d__3&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<CardPickViewData>.Start<ShopWindow.<LoadDrawCardAsync>d__36>(ShopWindow.<LoadDrawCardAsync>d__36&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>.Start<AddressableLoader.<AwaitScene>d__29>(AddressableLoader.<AwaitScene>d__29&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>.Start<AddressableLoader.<LoadScene>d__21>(AddressableLoader.<LoadScene>d__21&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>.Start<AddressableLoader.<ReloadScene>d__22>(AddressableLoader.<ReloadScene>d__22&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>.Start<SceneLoader.<LoadScene>d__1>(SceneLoader.<LoadScene>d__1&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>.Start<SceneLoader.<ReloadScene>d__2>(SceneLoader.<ReloadScene>d__2&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<byte>.Start<AChen.Player.PlayerSession.<>c__DisplayClass38_0.<<SendFriendRequestAsync>b__0>d>(AChen.Player.PlayerSession.<>c__DisplayClass38_0.<<SendFriendRequestAsync>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<byte>.Start<AChen.Player.PlayerSession.<>c__DisplayClass39_0.<<AcceptFriendRequestAsync>b__0>d>(AChen.Player.PlayerSession.<>c__DisplayClass39_0.<<AcceptFriendRequestAsync>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<byte>.Start<AChen.Player.PlayerSession.<>c__DisplayClass40_0.<<RejectFriendRequestAsync>b__0>d>(AChen.Player.PlayerSession.<>c__DisplayClass40_0.<<RejectFriendRequestAsync>b__0>d&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<byte>.Start<AChen.Player.PlayerSession.<SendAuthenticatedCallAsync>d__48<byte>>(AChen.Player.PlayerSession.<SendAuthenticatedCallAsync>d__48<byte>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<byte>.Start<AChen.Player.PlayerSession.<TryRestoreSessionAsync>d__26>(AChen.Player.PlayerSession.<TryRestoreSessionAsync>d__26&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<byte>.Start<AUIScreenController.<RunGuardedAsync>d__46>(AUIScreenController.<RunGuardedAsync>d__46&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Networking.AuthApi.<ClaimGiftAsync>d__23>(AChen.Networking.AuthApi.<ClaimGiftAsync>d__23&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Networking.AuthApi.<DrawCardsAsync>d__9>(AChen.Networking.AuthApi.<DrawCardsAsync>d__9&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Networking.AuthApi.<GetFriendsAsync>d__17>(AChen.Networking.AuthApi.<GetFriendsAsync>d__17&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Networking.AuthApi.<GetGachaPoolAsync>d__10>(AChen.Networking.AuthApi.<GetGachaPoolAsync>d__10&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Networking.AuthApi.<GetInboxAsync>d__22>(AChen.Networking.AuthApi.<GetInboxAsync>d__22&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Networking.AuthApi.<GetPlayerAsync>d__6>(AChen.Networking.AuthApi.<GetPlayerAsync>d__6&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Networking.AuthApi.<PostAuthAsync>d__24>(AChen.Networking.AuthApi.<PostAuthAsync>d__24&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Networking.AuthApi.<PurchaseShopItemAsync>d__8>(AChen.Networking.AuthApi.<PurchaseShopItemAsync>d__8&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Networking.AuthApi.<SearchFriendsAsync>d__18>(AChen.Networking.AuthApi.<SearchFriendsAsync>d__18&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Networking.AuthApi.<UpdateProfileAsync>d__7>(AChen.Networking.AuthApi.<UpdateProfileAsync>d__7&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Networking.BackendHttpClient.<SendAsync>d__4>(AChen.Networking.BackendHttpClient.<SendAsync>d__4&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Networking.BackendHttpClient.<SendAsync>d__5<object>>(AChen.Networking.BackendHttpClient.<SendAsync>d__5<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Player.PlayerSession.<ExecuteLockedAsync>d__47<object>>(AChen.Player.PlayerSession.<ExecuteLockedAsync>d__47<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Player.PlayerSession.<GetGachaPoolAsync>d__43>(AChen.Player.PlayerSession.<GetGachaPoolAsync>d__43&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Player.PlayerSession.<LoginAsync>d__25>(AChen.Player.PlayerSession.<LoginAsync>d__25&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Player.PlayerSession.<RegisterAsync>d__24>(AChen.Player.PlayerSession.<RegisterAsync>d__24&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Player.PlayerSession.<SendAuthenticatedAsync>d__49>(AChen.Player.PlayerSession.<SendAuthenticatedAsync>d__49&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Player.PlayerSession.<SendAuthenticatedCallAsync>d__48<object>>(AChen.Player.PlayerSession.<SendAuthenticatedCallAsync>d__48<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Player.PlayerSession.<SendAuthenticatedDrawAsync>d__51>(AChen.Player.PlayerSession.<SendAuthenticatedDrawAsync>d__51&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AChen.Player.PlayerSession.<SendAuthenticatedPoolAsync>d__50>(AChen.Player.PlayerSession.<SendAuthenticatedPoolAsync>d__50&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<AddressableLoader.<LoadAsset>d__28<object,object>>(AddressableLoader.<LoadAsset>d__28<object,object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<CardPoolAddress.<LoadAddressAsync>d__7>(CardPoolAddress.<LoadAddressAsync>d__7&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<CardPoolAddress.<LoadCardTextureAsync>d__6>(CardPoolAddress.<LoadCardTextureAsync>d__6&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<CardPoolAddress.<TryLoadAddressAsync>d__8>(CardPoolAddress.<TryLoadAddressAsync>d__8&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<CardPreviewWindow.<LoadCardImageAsync>d__16>(CardPreviewWindow.<LoadCardImageAsync>d__16&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<CardPreviewWindow.<LoadCardImagesAsync>d__15>(CardPreviewWindow.<LoadCardImagesAsync>d__15&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<GameFlow.<GetStartupSceneAsync>d__10>(GameFlow.<GetStartupSceneAsync>d__10&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<GridListController.<LoadRowPrefabAsync>d__13>(GridListController.<LoadRowPrefabAsync>d__13&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<ShopCatalogQuery.<LoadCardPacksAsync>d__0>(ShopCatalogQuery.<LoadCardPacksAsync>d__0&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<ShopCatalogQuery.<LoadCosmeticsAsync>d__2<object>>(ShopCatalogQuery.<LoadCosmeticsAsync>d__2<object>&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<ShopCatalogQuery.<LoadSpriteAsync>d__6>(ShopCatalogQuery.<LoadSpriteAsync>d__6&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<ShopCatalogQuery.<LoadSpritesAsync>d__5>(ShopCatalogQuery.<LoadSpritesAsync>d__5&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<ShopWindow.<LoadDrawCardsAsync>d__35>(ShopWindow.<LoadDrawCardsAsync>d__35&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,CardPreviewWindow.<LoadPoolAsync>d__14>(Cysharp.Threading.Tasks.UniTask.Awaiter&,CardPreviewWindow.<LoadPoolAsync>d__14&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,CardZoomWindow.<PlayCloseAsync>d__28>(Cysharp.Threading.Tasks.UniTask.Awaiter&,CardZoomWindow.<PlayCloseAsync>d__28&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,CardZoomWindow.<PlayOpenAsync>d__27>(Cysharp.Threading.Tasks.UniTask.Awaiter&,CardZoomWindow.<PlayOpenAsync>d__27&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,GameFlow.<EnterLobbyAsync>d__11>(Cysharp.Threading.Tasks.UniTask.Awaiter&,GameFlow.<EnterLobbyAsync>d__11&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,GameFlow.<ReturnToLoginAsync>d__7>(Cysharp.Threading.Tasks.UniTask.Awaiter&,GameFlow.<ReturnToLoginAsync>d__7&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,HotUpdateEntry.<BootAsync>d__2>(Cysharp.Threading.Tasks.UniTask.Awaiter&,HotUpdateEntry.<BootAsync>d__2&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,LogInWindow.InputFeedback.<PlayAsync>d__6>(Cysharp.Threading.Tasks.UniTask.Awaiter&,LogInWindow.InputFeedback.<PlayAsync>d__6&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,MessageWindow.<CloseAfterAsync>d__9>(Cysharp.Threading.Tasks.UniTask.Awaiter&,MessageWindow.<CloseAfterAsync>d__9&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,ShopWindow.<RefreshCurrentCategoryAsync>d__30>(Cysharp.Threading.Tasks.UniTask.Awaiter&,ShopWindow.<RefreshCurrentCategoryAsync>d__30&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,SingletonManager.<Start>d__2>(Cysharp.Threading.Tasks.UniTask.Awaiter&,SingletonManager.<Start>d__2&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<AChen.Player.AuthResult>,LogInWindow.<AuthenticateAsync>d__20>(Cysharp.Threading.Tasks.UniTask.Awaiter<AChen.Player.AuthResult>&,LogInWindow.<AuthenticateAsync>d__20&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>,GameFlow.<EnterLobbyAsync>d__11>(Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>&,GameFlow.<EnterLobbyAsync>d__11&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>,GameFlow.<ReturnToLoginAsync>d__7>(Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>&,GameFlow.<ReturnToLoginAsync>d__7&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>,SettingsWindow.<SwitchLanguageAsync>d__11>(Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>&,SettingsWindow.<SwitchLanguageAsync>d__11&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>,SingletonManager.<Start>d__2>(Cysharp.Threading.Tasks.UniTask.Awaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>&,SingletonManager.<Start>d__2&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<byte>,AvatarSelectWindow.<SubmitAsync>d__29>(Cysharp.Threading.Tasks.UniTask.Awaiter<byte>&,AvatarSelectWindow.<SubmitAsync>d__29&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<byte>,ChangeNameWindow.<SubmitAsync>d__12>(Cysharp.Threading.Tasks.UniTask.Awaiter<byte>&,ChangeNameWindow.<SubmitAsync>d__12&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<byte>,FriendWindow.<LoadFriendsAsync>d__14>(Cysharp.Threading.Tasks.UniTask.Awaiter<byte>&,FriendWindow.<LoadFriendsAsync>d__14&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<byte>,FriendWindow.<SearchAsync>d__15>(Cysharp.Threading.Tasks.UniTask.Awaiter<byte>&,FriendWindow.<SearchAsync>d__15&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<byte>,FriendWindow.<SendRequestAsync>d__16>(Cysharp.Threading.Tasks.UniTask.Awaiter<byte>&,FriendWindow.<SendRequestAsync>d__16&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<byte>,GiftWindow.<AcceptAsync>d__10>(Cysharp.Threading.Tasks.UniTask.Awaiter<byte>&,GiftWindow.<AcceptAsync>d__10&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<byte>,GiftWindow.<ClaimAsync>d__12>(Cysharp.Threading.Tasks.UniTask.Awaiter<byte>&,GiftWindow.<ClaimAsync>d__12&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<byte>,GiftWindow.<LoadInboxAsync>d__9>(Cysharp.Threading.Tasks.UniTask.Awaiter<byte>&,GiftWindow.<LoadInboxAsync>d__9&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<byte>,GiftWindow.<RejectAsync>d__11>(Cysharp.Threading.Tasks.UniTask.Awaiter<byte>&,GiftWindow.<RejectAsync>d__11&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<byte>,PreGameUIPanel.<SwitchToNextWallpaperAsync>d__38>(Cysharp.Threading.Tasks.UniTask.Awaiter<byte>&,PreGameUIPanel.<SwitchToNextWallpaperAsync>d__38&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<byte>,ShopWindow.<PurchaseAsync>d__33>(Cysharp.Threading.Tasks.UniTask.Awaiter<byte>&,ShopWindow.<PurchaseAsync>d__33&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,CardPreviewWindow.<LoadPoolAsync>d__14>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,CardPreviewWindow.<LoadPoolAsync>d__14&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,GiftRewardItem.<LoadArtAsync>d__6>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,GiftRewardItem.<LoadArtAsync>d__6&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,SceneEntry.<Start>d__3>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,SceneEntry.<Start>d__3&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,ShopWindow.<DrawPackAsync>d__34>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,ShopWindow.<DrawPackAsync>d__34&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,SingletonManager.<Start>d__2>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,SingletonManager.<Start>d__2&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,SocialPortrait.<ApplyAsync>d__0>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,SocialPortrait.<ApplyAsync>d__0&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.YieldAwaitable.Awaiter,CardZoomWindow.<PlayOpenAsync>d__27>(Cysharp.Threading.Tasks.YieldAwaitable.Awaiter&,CardZoomWindow.<PlayOpenAsync>d__27&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<LitMotion.MotionAwaiter,MessageWindow.<PlayOpenAsync>d__5>(LitMotion.MotionAwaiter&,MessageWindow.<PlayOpenAsync>d__5&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>,HotUpdateEntry.<BootAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>&,HotUpdateEntry.<BootAsync>d__2&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,AddressableLoader.<LoadCatalogsAsync>d__17>(System.Runtime.CompilerServices.TaskAwaiter<object>&,AddressableLoader.<LoadCatalogsAsync>d__17&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<AddressableLoader.<LoadCatalogsAsync>d__17>(AddressableLoader.<LoadCatalogsAsync>d__17&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<AvatarSelectWindow.<SubmitAsync>d__29>(AvatarSelectWindow.<SubmitAsync>d__29&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<CardPreviewWindow.<LoadPoolAsync>d__14>(CardPreviewWindow.<LoadPoolAsync>d__14&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<CardZoomWindow.<PlayCloseAsync>d__28>(CardZoomWindow.<PlayCloseAsync>d__28&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<CardZoomWindow.<PlayOpenAsync>d__27>(CardZoomWindow.<PlayOpenAsync>d__27&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<ChangeNameWindow.<SubmitAsync>d__12>(ChangeNameWindow.<SubmitAsync>d__12&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<FriendWindow.<LoadFriendsAsync>d__14>(FriendWindow.<LoadFriendsAsync>d__14&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<FriendWindow.<SearchAsync>d__15>(FriendWindow.<SearchAsync>d__15&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<FriendWindow.<SendRequestAsync>d__16>(FriendWindow.<SendRequestAsync>d__16&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<GameFlow.<EnterLobbyAsync>d__11>(GameFlow.<EnterLobbyAsync>d__11&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<GameFlow.<ReturnToLoginAsync>d__7>(GameFlow.<ReturnToLoginAsync>d__7&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<GiftRewardItem.<LoadArtAsync>d__6>(GiftRewardItem.<LoadArtAsync>d__6&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<GiftWindow.<AcceptAsync>d__10>(GiftWindow.<AcceptAsync>d__10&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<GiftWindow.<ClaimAsync>d__12>(GiftWindow.<ClaimAsync>d__12&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<GiftWindow.<LoadInboxAsync>d__9>(GiftWindow.<LoadInboxAsync>d__9&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<GiftWindow.<RejectAsync>d__11>(GiftWindow.<RejectAsync>d__11&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<HotUpdateEntry.<BootAsync>d__2>(HotUpdateEntry.<BootAsync>d__2&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<LogInWindow.<AuthenticateAsync>d__20>(LogInWindow.<AuthenticateAsync>d__20&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<LogInWindow.InputFeedback.<PlayAsync>d__6>(LogInWindow.InputFeedback.<PlayAsync>d__6&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<MessageWindow.<CloseAfterAsync>d__9>(MessageWindow.<CloseAfterAsync>d__9&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<MessageWindow.<PlayOpenAsync>d__5>(MessageWindow.<PlayOpenAsync>d__5&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<PreGameUIPanel.<SwitchToNextWallpaperAsync>d__38>(PreGameUIPanel.<SwitchToNextWallpaperAsync>d__38&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<SceneEntry.<Start>d__3>(SceneEntry.<Start>d__3&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<SettingsWindow.<SwitchLanguageAsync>d__11>(SettingsWindow.<SwitchLanguageAsync>d__11&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<ShopWindow.<DrawPackAsync>d__34>(ShopWindow.<DrawPackAsync>d__34&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<ShopWindow.<PurchaseAsync>d__33>(ShopWindow.<PurchaseAsync>d__33&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<ShopWindow.<RefreshCurrentCategoryAsync>d__30>(ShopWindow.<RefreshCurrentCategoryAsync>d__30&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<SingletonManager.<Start>d__2>(SingletonManager.<Start>d__2&)
		// System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<SocialPortrait.<ApplyAsync>d__0>(SocialPortrait.<ApplyAsync>d__0&)
		// Cysharp.Threading.Tasks.Internal.StateTuple<Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<object,object>>> Cysharp.Threading.Tasks.Internal.StateTuple.Create<Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<object,object>>>(Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<object,object>>)
		// Cysharp.Threading.Tasks.Internal.StateTuple<Cysharp.Threading.Tasks.UniTask.Awaiter<object>> Cysharp.Threading.Tasks.Internal.StateTuple.Create<Cysharp.Threading.Tasks.UniTask.Awaiter<object>>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>)
		// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<object,object>> Cysharp.Threading.Tasks.UniTask.FromCanceled<System.ValueTuple<object,object>>(System.Threading.CancellationToken)
		// Cysharp.Threading.Tasks.UniTask<object> Cysharp.Threading.Tasks.UniTask.FromCanceled<object>(System.Threading.CancellationToken)
		// Cysharp.Threading.Tasks.UniTask<object> Cysharp.Threading.Tasks.UniTask.FromResult<object>(object)
		// Cysharp.Threading.Tasks.UniTask<CardPickViewData[]> Cysharp.Threading.Tasks.UniTask.WhenAll<CardPickViewData>(Cysharp.Threading.Tasks.UniTask<CardPickViewData>[])
		// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<object,object>> Cysharp.Threading.Tasks.UniTask.WhenAll<object,object>(Cysharp.Threading.Tasks.UniTask<object>,Cysharp.Threading.Tasks.UniTask<object>)
		// Cysharp.Threading.Tasks.UniTask<object[]> Cysharp.Threading.Tasks.UniTask.WhenAll<object>(Cysharp.Threading.Tasks.UniTask<object>[])
		// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<object,object>> Cysharp.Threading.Tasks.UniTaskExtensions.AttachExternalCancellation<System.ValueTuple<object,object>>(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<object,object>>,System.Threading.CancellationToken)
		// Cysharp.Threading.Tasks.UniTask<object> Cysharp.Threading.Tasks.UniTaskExtensions.AttachExternalCancellation<object>(Cysharp.Threading.Tasks.UniTask<object>,System.Threading.CancellationToken)
		// Cysharp.Threading.Tasks.UniTask Cysharp.Threading.Tasks.UniTaskExtensions.ContinueWith<object>(Cysharp.Threading.Tasks.UniTask<object>,System.Action<object>)
		// System.Void Cysharp.Threading.Tasks.UniTaskExtensions.Forget<System.ValueTuple<object,object>>(Cysharp.Threading.Tasks.UniTask<System.ValueTuple<object,object>>)
		// System.Void Cysharp.Threading.Tasks.UniTaskExtensions.Forget<object>(Cysharp.Threading.Tasks.UniTask<object>)
		// System.Void LitMotion.Error.IsNull<object>(object)
		// LitMotion.MotionHandle LitMotion.Extensions.LitMotionRectTransformExtensions.BindToAnchoredPosition<LitMotion.NoOptions,LitMotion.Adapters.Vector2MotionAdapter>(LitMotion.MotionBuilder<UnityEngine.Vector2,LitMotion.NoOptions,LitMotion.Adapters.Vector2MotionAdapter>,UnityEngine.RectTransform)
		// LitMotion.MotionHandle LitMotion.Extensions.LitMotionTransformExtensions.BindToLocalScale<LitMotion.NoOptions,LitMotion.Adapters.Vector3MotionAdapter>(LitMotion.MotionBuilder<UnityEngine.Vector3,LitMotion.NoOptions,LitMotion.Adapters.Vector3MotionAdapter>,UnityEngine.Transform)
		// LitMotion.MotionHandle LitMotion.Extensions.LitMotionUGUIExtensions.BindToAlpha<LitMotion.NoOptions,LitMotion.Adapters.FloatMotionAdapter>(LitMotion.MotionBuilder<float,LitMotion.NoOptions,LitMotion.Adapters.FloatMotionAdapter>,UnityEngine.CanvasGroup)
		// LitMotion.MotionHandle LitMotion.Extensions.LitMotionUGUIExtensions.BindToColorA<LitMotion.NoOptions,LitMotion.Adapters.FloatMotionAdapter>(LitMotion.MotionBuilder<float,LitMotion.NoOptions,LitMotion.Adapters.FloatMotionAdapter>,UnityEngine.UI.Graphic)
		// LitMotion.MotionHandle LitMotion.MotionBuilder<UnityEngine.Vector2,LitMotion.NoOptions,LitMotion.Adapters.Vector2MotionAdapter>.Bind<object>(object,System.Action<UnityEngine.Vector2,object>)
		// LitMotion.MotionHandle LitMotion.MotionBuilder<UnityEngine.Vector3,LitMotion.NoOptions,LitMotion.Adapters.Vector3MotionAdapter>.Bind<object>(object,System.Action<UnityEngine.Vector3,object>)
		// LitMotion.MotionHandle LitMotion.MotionBuilder<float,LitMotion.NoOptions,LitMotion.Adapters.FloatMotionAdapter>.Bind<object>(object,System.Action<float,object>)
		// System.Void LitMotion.MotionBuilder<UnityEngine.Vector2,LitMotion.NoOptions,LitMotion.Adapters.Vector2MotionAdapter>.SetCallbackData<object>(object,System.Action<UnityEngine.Vector2,object>)
		// System.Void LitMotion.MotionBuilder<UnityEngine.Vector3,LitMotion.NoOptions,LitMotion.Adapters.Vector3MotionAdapter>.SetCallbackData<object>(object,System.Action<UnityEngine.Vector3,object>)
		// System.Void LitMotion.MotionBuilder<float,LitMotion.NoOptions,LitMotion.Adapters.FloatMotionAdapter>.SetCallbackData<object>(object,System.Action<float,object>)
		// object Newtonsoft.Json.JsonConvert.DeserializeObject<object>(string,Newtonsoft.Json.JsonSerializerSettings)
		// CardPickViewData[] System.Array.Empty<CardPickViewData>()
		// int[] System.Array.Empty<int>()
		// object[] System.Array.Empty<object>()
		// int System.Array.IndexOf<int>(int[],int)
		// int System.Array.IndexOfImpl<int>(int[],int,int,int)
		// bool System.Linq.Enumerable.Any<ushort>(System.Collections.Generic.IEnumerable<ushort>,System.Func<ushort,bool>)
		// bool System.Linq.Enumerable.Contains<int>(System.Collections.Generic.IEnumerable<int>,int)
		// bool System.Linq.Enumerable.Contains<int>(System.Collections.Generic.IEnumerable<int>,int,System.Collections.Generic.IEqualityComparer<int>)
		// System.Linq.IOrderedEnumerable<int> System.Linq.Enumerable.OrderBy<int,int>(System.Collections.Generic.IEnumerable<int>,System.Func<int,int>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.Enumerable.OrderBy<object,int>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.Enumerable.OrderBy<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,object>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.Enumerable.OrderBy<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,object>,System.Collections.Generic.IComparer<object>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Select<object,int>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<System.Collections.Generic.KeyValuePair<object,object>,object>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>,System.Func<System.Collections.Generic.KeyValuePair<object,object>,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,object>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.Enumerable.ThenBy<object,int>(System.Linq.IOrderedEnumerable<object>,System.Func<object,int>)
		// int[] System.Linq.Enumerable.ToArray<int>(System.Collections.Generic.IEnumerable<int>)
		// object[] System.Linq.Enumerable.ToArray<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Where<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Iterator<object>.Select<int>(System.Func<object,int>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<System.Collections.Generic.KeyValuePair<object,object>>.Select<object>(System.Func<System.Collections.Generic.KeyValuePair<object,object>,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<object>.Select<object>(System.Func<object,object>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.IOrderedEnumerable<object>.CreateOrderedEnumerable<int>(System.Func<object,int>,System.Collections.Generic.IComparer<int>,bool)
		// object& System.Runtime.CompilerServices.Unsafe.As<object,object>(object&)
		// System.Void* System.Runtime.CompilerServices.Unsafe.AsPointer<object>(object&)
		// object& Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<object,object>(object&)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<object>(UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<object>(object)
		// System.Void UnityEngine.AddressableAssets.Addressables.Release<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>)
		// System.Void UnityEngine.AddressableAssets.Addressables.Release<long>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<long>)
		// System.Void UnityEngine.AddressableAssets.Addressables.Release<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.AddressableAssets.AddressablesImpl.LoadAssetAsync<object>(UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.AddressableAssets.AddressablesImpl.LoadAssetAsync<object>(object)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.AddressableAssets.AddressablesImpl.LoadAssetWithChain<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.AddressableAssets.AddressablesImpl.LoadAssetWithChain<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,object)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.AddressableAssets.AddressablesImpl.TrackHandle<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>)
		// object UnityEngine.Component.GetComponent<object>()
		// object UnityEngine.Component.GetComponentInChildren<object>()
		// object UnityEngine.Component.GetComponentInChildren<object>(bool)
		// object UnityEngine.Component.GetComponentInParent<object>()
		// object[] UnityEngine.Component.GetComponentsInChildren<object>(bool)
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>(bool)
		// object[] UnityEngine.GameObject.GetComponentsInChildren<object>(bool)
		// System.Array UnityEngine.GameObject.GetComponentsInternal<object>(bool,bool,bool,bool,System.Collections.Generic.List<object>)
		// object UnityEngine.Object.FindFirstObjectByType<object>()
		// object UnityEngine.Object.Instantiate<object>(object)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform,bool)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.Convert<object>()
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.ResourceManager.CreateChainOperation<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,System.Func<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object>>)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.ResourceManager.CreateCompletedOperationInternal<object>(object,bool,System.Exception,bool)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.ResourceManager.CreateCompletedOperationWithException<object>(object,System.Exception)
		// object UnityEngine.ResourceManagement.ResourceManager.CreateOperation<object>(System.Type,int,UnityEngine.ResourceManagement.Util.IOperationCacheKey,System.Action<UnityEngine.ResourceManagement.AsyncOperations.IAsyncOperation>)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.ResourceManager.ProvideResource<object>(UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation)
		// UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> UnityEngine.ResourceManagement.ResourceManager.StartOperation<object>(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase<object>,UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle)
		// object UnityEngine.Resources.Load<object>(string)
	}
}