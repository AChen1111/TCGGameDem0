---@class PreGamePanel : BasePanel
---@field m_BtnPlay UnityEngine.UI.Button
---@field m_BtnDeck UnityEngine.UI.Button
---@field m_BtnShop UnityEngine.UI.Button
---@field m_BtnExit UnityEngine.UI.Button
---@field m_BtnGift UnityEngine.UI.Button
---@field m_BtnFriend UnityEngine.UI.Button
---@field m_BtnMail UnityEngine.UI.Button
---@field m_BtnSetting UnityEngine.UI.Button

local PreGamePanel = {}
PreGamePanel.__index = PreGamePanel
setmetatable(PreGamePanel, BasePanel)
_G.PreGamePanel = PreGamePanel

function PreGamePanel:AddListeners()
    self.m_uiComp:AddClick(self.m_BtnPlay, "OnPlayBtnClicked")
    self.m_uiComp:AddClick(self.m_BtnDeck, "OnDeckBtnClicked")
    self.m_uiComp:AddClick(self.m_BtnShop, "OnShopBtnClicked")
    self.m_uiComp:AddClick(self.m_BtnExit, "OnExitBtnClicked")
    self.m_uiComp:AddClick(self.m_BtnGift, "OnGiftBtnClicked")
    self.m_uiComp:AddClick(self.m_BtnFriend, "OnFriendBtnClicked")
    self.m_uiComp:AddClick(self.m_BtnMail, "OnMailBtnClicked")
    self.m_uiComp:AddClick(self.m_BtnSetting, "OnSettingBtnClicked")
end

function PreGamePanel:OnPlayBtnClicked()
    Log.Info("OnPlayBtnClicked2")
end

function PreGamePanel:OnDeckBtnClicked()
    Log.Info("OnDeckBtnClicked")
end

function PreGamePanel:OnShopBtnClicked()
    Log.Info("OnShopBtnClicked")
end

function PreGamePanel:OnExitBtnClicked()
    Log.Info("OnExitBtnClicked")
end

function PreGamePanel:OnGiftBtnClicked()
    Log.Info("OnGiftBtnClicked")
end

function PreGamePanel:OnFriendBtnClicked()
    Log.Info("OnFriendBtnClicked")
end

function PreGamePanel:OnMailBtnClicked()
    Log.Info("OnMailBtnClicked")
end

function PreGamePanel:OnSettingBtnClicked()
    Log.Info("OnSettingBtnClicked")
end