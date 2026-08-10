BaseUI = {}
BaseUI.__index = BaseUI

function BaseUI:Awake()
    print("BaseUI Awake")
end

function BaseUI:OnClicked()
    if self.m_Button == nil then
        print("BaseUI: m_Button is nil, check LuaComponet ObjectReference name/value")
        return
    end
    print("hello")
    self.m_Button.image.color = Color.red
end



