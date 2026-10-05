using Godot;
using System.Collections.Generic;
namespace Realmshift;
public partial class AudioManager : Node
{
    private AudioStreamPlayer _music=null!;
    private readonly AudioStreamPlayer[] _voices=new AudioStreamPlayer[12];
    private readonly Dictionary<string,AudioStream> _sounds=new();
    private float _hitThrottle;
    private int _theme=-1;
    private AudioStream? _themeStream;
    public override void _Ready()
    {
        _music=new AudioStreamPlayer();AddChild(_music);_music.Finished+=()=>_music.Play();
        for(int i=0;i<_voices.Length;i++){_voices[i]=new AudioStreamPlayer();AddChild(_voices[i]);}
        foreach(string s in new[]{"shoot","bow","blade","cast","hit","dash","pickup","level","ui","hurt","shift","boss"})_sounds[s]=GD.Load<AudioStream>($"res://Assets/Audio/SFX/{s}.wav");
        ApplyVolumes();
    }
    public void ApplyVolumes()=>_music.VolumeDb=Mathf.LinearToDb(Mathf.Max(.0001f,SaveManager.Data.Settings.Master*SaveManager.Data.Settings.Music));
    public void Music(int theme)
    {if(_theme==theme)return;_theme=theme;_music.Stop();_music.Stream=null;_themeStream?.Dispose();_themeStream=GD.Load<AudioStream>($"res://Assets/Audio/Music/theme_{theme}.wav");_music.Stream=_themeStream;_music.Play();}
    public void Play(string name,float volume=1)
    {
        if(name=="hit"&&_hitThrottle>0)return;if(name=="hit")_hitThrottle=.04f;
        foreach(var v in _voices)if(!v.Playing){v.Stream=_sounds[name];v.PitchScale=name=="ui"?1:(float)GD.RandRange(.93,1.06);v.VolumeDb=Mathf.LinearToDb(Mathf.Max(.0001f,SaveManager.Data.Settings.Master*SaveManager.Data.Settings.Sfx*volume));v.Play();break;}
    }
    public override void _Process(double delta){_hitThrottle=Mathf.Max(0,_hitThrottle-(float)delta);}
    public void StopAll()
    {_music.Stop();_music.Stream=null;foreach(var v in _voices){v.Stop();v.Stream=null;}}
    public override void _ExitTree()
    {
        StopAll();
        foreach(var stream in _sounds.Values)stream.Dispose();_sounds.Clear();_themeStream?.Dispose();_themeStream=null;
    }
}
