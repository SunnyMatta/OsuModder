
# OsuModder

>[!WARNING]
> This is Early Access.
>If you encountered any problems or bugs, please open issue or pr <3

<img align="left" width="256" height="256" src="Images/lazermod.png">
Mod-Loader for osu!lazer, written without external dependencies.

<h1 style="clear: both;">Why?</h1>

Recently I was curious to modificate UI for osu! to get some additional functionality, also to give it to my friend for some giggles. However, Recompiling osu!'s dlls and sending back asking to delete specific files for every update is just so annoying. I love playing with games from assembly level, so I've decided to make entire mod-loader for it. *Really hope you won't call it useless. Also, peppy pwease don't hate me <3*

## Usage
You basically just putting `.dll` file (which is mod) into the `mods/` folder.

>[!NOTE]
>You must have `OsuModderApi.dll` in the mod folder, if mod requires it. It should be included with mod.

Code will generate `mods/` folder in:

 - Linux:
   	 -  Appimage (Recommended): 
   	`~/.local/share/osu/mods`
   	  - Flatpak:
*No support yet*
 - Windows:
`C:\Users\{username}\AppData\Local\osulazer\mods\`

## Example
OsuModder heavily depends on osu!'s libraries.

Please check [osu!Framework Documentation](https://github.com/ppy/osu-framework/wiki/Setting-up-your-first-project) in order to get started.

Anyway, let me show you an example:

	using osu.Framework.Audio;
	using osu.Framework.Graphics;
	using osu.Framework.Graphics.Containers;
	using osu.Game.Audio;
	using osu.Game.Graphics.UserInterface;
	using osu.Game.Scoring;
	using osu.Game.Screens.Ranking;
	using osu.Game.Skinning;
	
	namespace osuMod
	{
		public partial class EpicCustomResultsScreen: ResultsScreen
		{
			public EpicCustomResultsScreen(ScoreInfo score): base(score)
			{
			}
			protected override void LoadComplete()
			{
				base.LoadComplete();
				
				// clears any internals from ResultScreen.
				ClearInternal();
				
				// initializing list of strings.
				var applauseSamples = new List<string>();
				
				// just a volume for applause
				const double applause_volume = 0.8f;
				
				// adding applause-s (version of applause which playing when you hitting s/ss score) sound to the list
				applauseSamples.Add(@"Results/applause-s");
				
				// initializing audio component
				var rankApplauseSound = new PoolableSkinnableSample(
				    new SampleInfo(applauseSamples.ToArray())
				);
				
				// AddInternal(); used for adding components to the scene you modding
				AddInternal(rankApplauseSound);
				AddInternal(new Container
				            {
					AutoSizeAxes = Axes.Both,
					Children = new Drawable[]
						   {
					new CustomOsuButton
					{
					Position = new osuTK.Vector2(10,10),
					Width = 200,
					Text = "meow",
					Action = () =>
						{
						rankApplauseSound.VolumeTo(applause_volume);
						rankApplauseSound.Play();
						}
				        }
				}
				});
			}
			private  partial  class  CustomOsuButton : OsuButton
			{
			}
		}
	}
So, that code affects ResultsScreen which adds button with function and removes everything like scores and stuff.
the `Action = () => ` from code simply means that if button will be clicked, it will execute that scope (for this example we used applause sound).

This is just very simple example of osu!Framework, which can be compiled and used as a mod for osu!

