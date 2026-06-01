using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs;

namespace ProperSkillDistributor
{
    [PrefabExtension("CharacterDeveloper", "descendant::Window/Widget/Children")]
    public class CharacterScreenPresetButtonsPatch : PrefabExtensionInsertPatch
    {
        public override string Id => "SkillPresetCharacterDeveloperButtons";

        public override int Position => PositionLast;

        public override XmlDocument GetPrefabExtension()
        {
            XmlDocument document = new XmlDocument();

            document.LoadXml(@"
                <Widget Id=""SkillPresetButtonLayer"" DoNotAcceptEvents=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" IsVisible=""@IsSkillPresetControlsVisible"">
                  <Children>

                    <ListPanel DoNotAcceptEvents=""true"" WidthSizePolicy=""CoverChildren"" HeightSizePolicy=""CoverChildren"" StackLayout.LayoutMethod=""HorizontalLeftToRight"" HorizontalAlignment=""Left"" VerticalAlignment=""Top"" MarginTop=""30"" MarginLeft=""40"">
                      <Children>
                        <ButtonWidget Id=""SkillPresetEditButton"" DoNotPassEventsToChildren=""true"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed"" SuggestedWidth=""155"" SuggestedHeight=""41"" Brush=""Character.Selection.Button"" Command.Click=""ExecuteOpenPresetEditorSelector"" UpdateChildrenStates=""true"">
                          <Children>
                            <TextWidget DoNotAcceptEvents=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" Brush=""CharacterDeveloper.Title.Text"" Brush.FontSize=""18"" Text=""Add Presets"" />
                          </Children>
                        </ButtonWidget>
                      </Children>
                    </ListPanel>

                    <Widget DoNotAcceptEvents=""true"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed"" SuggestedWidth=""290"" SuggestedHeight=""430"" HorizontalAlignment=""Left"" VerticalAlignment=""Top"" MarginTop=""30"" MarginLeft=""210"">
                      <Children>

                        <ButtonWidget Id=""SkillPresetUseButton"" DoNotPassEventsToChildren=""true"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed"" SuggestedWidth=""290"" SuggestedHeight=""41"" HorizontalAlignment=""Left"" VerticalAlignment=""Top"" Brush=""Character.Selection.Button"" Command.Click=""ExecuteToggleUsePresetDropdown"" UpdateChildrenStates=""true"">
                          <Children>
                            <ListPanel DoNotAcceptEvents=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" StackLayout.LayoutMethod=""HorizontalLeftToRight"" MarginLeft=""12"" MarginRight=""10"">
                              <Children>
                                <TextWidget DoNotAcceptEvents=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" Brush=""CharacterDeveloper.Title.Text"" Brush.FontSize=""18"" Brush.TextHorizontalAlignment=""Left"" Text=""@UsePresetsText"" />
                                <TextWidget DoNotAcceptEvents=""true"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""28"" Brush=""CharacterDeveloper.Title.Text"" Brush.FontSize=""22"" Text=""∨"" />
                              </Children>
                            </ListPanel>
                          </Children>
                        </ButtonWidget>

                        <BrushWidget Id=""SkillPresetUseDropdownPanel"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed"" SuggestedWidth=""290"" SuggestedHeight=""360"" HorizontalAlignment=""Left"" VerticalAlignment=""Top"" MarginTop=""45"" Brush=""Frame1Brush"" IsVisible=""@IsUsePresetDropdownOpen"">
                          <Children>

                            <ScrollablePanel Id=""SkillPresetUseScrollablePanel"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" MarginTop=""12"" MarginBottom=""12"" MarginLeft=""10"" MarginRight=""10"" AutoHideScrollBars=""true"" ClipRect=""SkillPresetUseClip"" InnerPanel=""SkillPresetUseClip\SkillPresetUseList"" VerticalScrollbar=""..\SkillPresetUseScrollbar"">
                              <Children>
                                <Widget Id=""SkillPresetUseClip"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" ClipContents=""true"">
                                  <Children>
                                    <NavigatableListPanel Id=""SkillPresetUseList"" DataSource=""{UsePresetRows}"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""CoverChildren"" StackLayout.LayoutMethod=""VerticalBottomToTop"">
                                      <ItemTemplate>
                                        <ButtonWidget DoNotPassEventsToChildren=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""Fixed"" SuggestedHeight=""48"" MarginBottom=""4"" Brush=""Character.Selection.Button"" ButtonType=""Radio"" IsSelected=""@IsSelected"" Command.Click=""ExecuteSelect"" UpdateChildrenStates=""true"">
                                          <Children>
                                            <ListPanel DoNotAcceptEvents=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" MarginLeft=""10"" MarginRight=""10"" StackLayout.LayoutMethod=""VerticalBottomToTop"">
                                              <Children>
                                                <TextWidget DoNotAcceptEvents=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""Fixed"" SuggestedHeight=""25"" Brush=""CharacterDeveloper.Title.Text"" Brush.FontSize=""20"" Brush.TextHorizontalAlignment=""Left"" Text=""@NameText"" />
                                                <TextWidget DoNotAcceptEvents=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""Fixed"" SuggestedHeight=""18"" Brush=""CharacterDeveloper.GridSkillName.Text"" Brush.FontSize=""14"" Brush.TextHorizontalAlignment=""Left"" Text=""@StatusText"" />
                                              </Children>
                                            </ListPanel>
                                          </Children>
                                        </ButtonWidget>
                                      </ItemTemplate>
                                    </NavigatableListPanel>
                                  </Children>
                                </Widget>
                              </Children>
                            </ScrollablePanel>

                            <ScrollbarWidget Id=""SkillPresetUseScrollbar"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""6"" HorizontalAlignment=""Right"" MarginTop=""12"" MarginBottom=""12"" MarginRight=""4"" AlignmentAxis=""Vertical"" Handle=""SkillPresetUseScrollbarHandle"" MaxValue=""100"" MinValue=""0"">
                              <Children>
                                <Widget WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""2"" HorizontalAlignment=""Center"" Sprite=""BlankWhiteSquare_9"" Color=""#5a4033FF"" AlphaFactor=""0.2"" />
                                <ImageWidget Id=""SkillPresetUseScrollbarHandle"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed"" SuggestedHeight=""10"" SuggestedWidth=""6"" HorizontalAlignment=""Center"" Brush=""FaceGen.Scrollbar.Handle"" />
                              </Children>
                            </ScrollbarWidget>

                          </Children>
                        </BrushWidget>

                      </Children>
                    </Widget>

                  </Children>
                </Widget>");

            return document;
        }
    }
}