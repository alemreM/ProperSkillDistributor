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

                    <ButtonWidget Id=""SkillPresetDropdownCloseCatcher"" DoNotPassEventsToChildren=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" Command.Click=""ExecuteCloseUsePresetDropdown"" IsVisible=""@IsUsePresetDropdownOpen"" />

                    <ListPanel DoNotAcceptEvents=""true"" WidthSizePolicy=""CoverChildren"" HeightSizePolicy=""CoverChildren"" StackLayout.LayoutMethod=""HorizontalLeftToRight"" HorizontalAlignment=""Left"" VerticalAlignment=""Top"" MarginTop=""30"" MarginLeft=""40"">
                      <Children>

                        <Widget Id=""SkillPresetEditButtonFrame"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed"" SuggestedWidth=""155"" SuggestedHeight=""41"" Sprite=""BlankWhiteSquare_9"" Color=""#000000B8"">
                          <Children>
                            <Widget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""Fixed"" SuggestedHeight=""1"" VerticalAlignment=""Top"" Sprite=""BlankWhiteSquare_9"" Color=""#6E5A49DD"" />
                            <Widget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""Fixed"" SuggestedHeight=""1"" VerticalAlignment=""Bottom"" Sprite=""BlankWhiteSquare_9"" Color=""#6E5A49DD"" />
                            <Widget WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""1"" HorizontalAlignment=""Left"" Sprite=""BlankWhiteSquare_9"" Color=""#6E5A49DD"" />
                            <Widget WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""1"" HorizontalAlignment=""Right"" Sprite=""BlankWhiteSquare_9"" Color=""#6E5A49DD"" />

                            <ButtonWidget Id=""SkillPresetEditButton"" DoNotPassEventsToChildren=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" Command.Click=""ExecuteOpenPresetEditorSelector"" UpdateChildrenStates=""true"">
                              <Children>
                                <TextWidget DoNotAcceptEvents=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" Brush=""CharacterDeveloper.Title.Text"" Brush.FontSize=""18"" Text=""Add Presets"" />
                              </Children>
                            </ButtonWidget>
                          </Children>
                        </Widget>

                      </Children>
                    </ListPanel>

                    <Widget DoNotAcceptEvents=""true"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed"" SuggestedWidth=""290"" SuggestedHeight=""430"" HorizontalAlignment=""Left"" VerticalAlignment=""Top"" MarginTop=""30"" MarginLeft=""210"">
                      <Children>

                        <Widget Id=""SkillPresetUseButtonFrame"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed"" SuggestedWidth=""290"" SuggestedHeight=""41"" HorizontalAlignment=""Left"" VerticalAlignment=""Top"" Sprite=""BlankWhiteSquare_9"" Color=""#000000B8"">
                          <Children>
                            <Widget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""Fixed"" SuggestedHeight=""1"" VerticalAlignment=""Top"" Sprite=""BlankWhiteSquare_9"" Color=""#6E5A49DD"" />
                            <Widget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""Fixed"" SuggestedHeight=""1"" VerticalAlignment=""Bottom"" Sprite=""BlankWhiteSquare_9"" Color=""#6E5A49DD"" />
                            <Widget WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""1"" HorizontalAlignment=""Left"" Sprite=""BlankWhiteSquare_9"" Color=""#6E5A49DD"" />
                            <Widget WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""1"" HorizontalAlignment=""Right"" Sprite=""BlankWhiteSquare_9"" Color=""#6E5A49DD"" />

                            <ButtonWidget Id=""SkillPresetUseButton"" DoNotPassEventsToChildren=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" Command.Click=""ExecuteToggleUsePresetDropdown"" UpdateChildrenStates=""true"">
                              <Children>
                                <ListPanel DoNotAcceptEvents=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" StackLayout.LayoutMethod=""HorizontalLeftToRight"" MarginLeft=""12"" MarginRight=""10"">
                                  <Children>
                                    <TextWidget DoNotAcceptEvents=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" Brush=""CharacterDeveloper.Title.Text"" Brush.FontSize=""18"" Brush.TextHorizontalAlignment=""Left"" Text=""@UsePresetsText"" />
                                    <TextWidget DoNotAcceptEvents=""true"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""28"" Brush=""CharacterDeveloper.Title.Text"" Brush.FontSize=""22"" Text=""∨"" />
                                  </Children>
                                </ListPanel>
                              </Children>
                            </ButtonWidget>
                          </Children>
                        </Widget>

                        <Widget Id=""SkillPresetUseDropdownPanel"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed"" SuggestedWidth=""290"" SuggestedHeight=""360"" HorizontalAlignment=""Left"" VerticalAlignment=""Top"" MarginTop=""45"" Sprite=""BlankWhiteSquare_9"" Color=""#000000B8"" IsVisible=""@IsUsePresetDropdownOpen"">
                          <Children>

                            <Widget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""Fixed"" SuggestedHeight=""1"" VerticalAlignment=""Top"" Sprite=""BlankWhiteSquare_9"" Color=""#6E5A49DD"" />
                            <Widget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""Fixed"" SuggestedHeight=""1"" VerticalAlignment=""Bottom"" Sprite=""BlankWhiteSquare_9"" Color=""#6E5A49DD"" />
                            <Widget WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""1"" HorizontalAlignment=""Left"" Sprite=""BlankWhiteSquare_9"" Color=""#6E5A49DD"" />
                            <Widget WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""1"" HorizontalAlignment=""Right"" Sprite=""BlankWhiteSquare_9"" Color=""#6E5A49DD"" />

                            <ScrollablePanel Id=""SkillPresetUseScrollablePanel"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" MarginTop=""6"" MarginBottom=""6"" MarginLeft=""8"" MarginRight=""8"" AutoHideScrollBars=""true"" ClipRect=""SkillPresetUseClip"" InnerPanel=""SkillPresetUseClip\SkillPresetUseList"" VerticalScrollbar=""..\SkillPresetUseScrollbar"">
                              <Children>
                                <Widget Id=""SkillPresetUseClip"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" ClipContents=""true"">
                                  <Children>
                                    <NavigatableListPanel Id=""SkillPresetUseList"" DataSource=""{UsePresetRows}"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""CoverChildren"" StackLayout.LayoutMethod=""VerticalBottomToTop"">
                                      <ItemTemplate>
                                        <Widget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""Fixed"" SuggestedHeight=""48"" MarginBottom=""4"" Sprite=""BlankWhiteSquare_9"" Color=""#00000082"">
                                          <Children>
                                            <Widget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""Fixed"" SuggestedHeight=""1"" VerticalAlignment=""Top"" Sprite=""BlankWhiteSquare_9"" Color=""#5E4B3CDD"" />
                                            <Widget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""Fixed"" SuggestedHeight=""1"" VerticalAlignment=""Bottom"" Sprite=""BlankWhiteSquare_9"" Color=""#5E4B3CDD"" />

                                            <ButtonWidget DoNotPassEventsToChildren=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" ButtonType=""Radio"" IsSelected=""@IsSelected"" Command.Click=""ExecuteSelect"" Command.HoverBegin=""ExecuteHoverBegin"" Command.HoverEnd=""ExecuteHoverEnd"" UpdateChildrenStates=""true"">
                                              <Children>
                                                <ListPanel DoNotAcceptEvents=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" MarginLeft=""8"" MarginRight=""10"" StackLayout.LayoutMethod=""HorizontalLeftToRight"">
                                                  <Children>

                                                    <TextWidget DoNotAcceptEvents=""true"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""16"" Brush=""CharacterDeveloper.Title.Text"" Brush.FontSize=""22"" Brush.TextHorizontalAlignment=""Center"" Text=""&gt;"" IsVisible=""@IsHovered"" />

                                                    <ListPanel DoNotAcceptEvents=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""StretchToParent"" StackLayout.LayoutMethod=""VerticalBottomToTop"">
                                                      <Children>
                                                        <TextWidget DoNotAcceptEvents=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""Fixed"" SuggestedHeight=""25"" Brush=""CharacterDeveloper.Title.Text"" Brush.FontSize=""20"" Brush.TextHorizontalAlignment=""Left"" Text=""@NameText"" />
                                                        <TextWidget DoNotAcceptEvents=""true"" WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""Fixed"" SuggestedHeight=""18"" Brush=""CharacterDeveloper.GridSkillName.Text"" Brush.FontSize=""14"" Brush.TextHorizontalAlignment=""Left"" Text=""@StatusText"" />
                                                      </Children>
                                                    </ListPanel>

                                                  </Children>
                                                </ListPanel>
                                              </Children>
                                            </ButtonWidget>
                                          </Children>
                                        </Widget>
                                      </ItemTemplate>
                                    </NavigatableListPanel>
                                  </Children>
                                </Widget>
                              </Children>
                            </ScrollablePanel>

                            <ScrollbarWidget Id=""SkillPresetUseScrollbar"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""6"" HorizontalAlignment=""Right"" MarginTop=""6"" MarginBottom=""6"" MarginRight=""4"" AlignmentAxis=""Vertical"" Handle=""SkillPresetUseScrollbarHandle"" MaxValue=""100"" MinValue=""0"">
                              <Children>
                                <Widget WidthSizePolicy=""Fixed"" HeightSizePolicy=""StretchToParent"" SuggestedWidth=""2"" HorizontalAlignment=""Center"" Sprite=""BlankWhiteSquare_9"" Color=""#5a4033FF"" AlphaFactor=""0.2"" />
                                <ImageWidget Id=""SkillPresetUseScrollbarHandle"" WidthSizePolicy=""Fixed"" HeightSizePolicy=""Fixed"" SuggestedHeight=""10"" SuggestedWidth=""6"" HorizontalAlignment=""Center"" Brush=""FaceGen.Scrollbar.Handle"" />
                              </Children>
                            </ScrollbarWidget>

                          </Children>
                        </Widget>

                      </Children>
                    </Widget>

                  </Children>
                </Widget>");

            return document;
        }
    }
}