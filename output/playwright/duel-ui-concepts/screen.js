// 静态视觉提案。全部房间、账号、回放、人数与战绩均为示意数据。
// 此文件不接入游戏，也不属于 Unity 功能代码。
const params = new URLSearchParams(location.search);
const variant = params.get('v') || 'a';
const mode = params.get('screen') || 'room';
const screen = document.querySelector('#screen');
screen.className = `screen variant-${variant} mode-${mode}`;

const image = (name, cls = '', alt = '') => `<img class="${cls}" src="assets/${name}.png" alt="${alt}">`;
const avatar = (n, cls = '') => `<div class="avatar ${cls}">${image(`avatar-${n}`)}</div>`;
const tag = (label, type = '') => `<span class="tag ${type}">${label}</span>`;
const action = (label, cls = '', icon = '') => `<div class="action ${cls}">${icon ? image(icon, 'action-icon') : ''}<span>${label}</span></div>`;
const field = (n, cls = '') => image(`field-${n}`, `field-art ${cls}`);
const art = (n, cls = '') => image(`art-${n}`, `monster-art ${cls}`);
const rooms = [
  {name:'今晚来一场友谊决斗', host:'AChen', avatar:2, id:'820 461', people:'4 / 8', kind:'休闲决斗', state:'等待中', note:'欢迎加入，自由切磋牌组', art:1, field:1},
  {name:'大师规则 · 自由练习', host:'星尘', avatar:12, id:'820 328', people:'6 / 8', kind:'标准规则', state:'等待中', note:'练习展开，也欢迎观战', art:11, field:2},
  {name:'HERO 专题交流', host:'游城十代', avatar:11, id:'819 952', people:'2 / 4', kind:'休闲决斗', state:'决斗中', note:'英雄卡组同好集合', art:5, field:3},
  {name:'周末决斗大会', host:'夜行', avatar:17, id:'819 774', people:'8 / 8', kind:'标准规则', state:'已满员', note:'高手对局，开放观战', art:25, field:5},
];
const replays = [
  {name:'青眼的逆转一击', rival:'星尘', avatar:12, result:'胜利', turns:8, time:'12:36', date:'10/03 20:42', art:1, other:11, field:1, deck:'青眼白龙', rivalDeck:'星尘同调'},
  {name:'HERO 的终场', rival:'游城十代', avatar:11, result:'失败', turns:6, time:'09:18', date:'10/03 19:15', art:5, other:17, field:2, deck:'元素英雄', rivalDeck:'黑魔导'},
  {name:'魔法师的连锁', rival:'夜行', avatar:22, result:'胜利', turns:11, time:'18:04', date:'10/02 22:06', art:2, other:24, field:3, deck:'黑魔导', rivalDeck:'电子龙'},
  {name:'白龙与银河', rival:'银河', avatar:24, result:'胜利', turns:5, time:'07:52', date:'10/02 21:30', art:32, other:19, field:6, deck:'青眼白龙', rivalDeck:'银河眼'},
];

function header() {
  return `<header class="topbar"><div class="brand-area"><div class="back">${image('back')}</div><div class="brand">DUEL<span>${mode === 'room' ? '决斗大厅' : '决斗回放'}</span></div></div><nav class="topnav"><a class="${mode==='room'?'active':''}" href="?v=${variant}&screen=room">房间选择</a><a class="${mode==='replay'?'active':''}" href="?v=${variant}&screen=replay">回放查看</a></nav><div class="account">${avatar(2)}<div><strong>AChen</strong><span>决斗者 ID　100 268 415</span></div></div></header>`;
}
function title(kicker, title, desc, suffix='') {
  return `<div class="page-title"><div><div class="eyebrow">${kicker}</div><h1>${title}</h1><p>${desc}</p></div>${suffix}</div>`;
}
function filters(replay=false) {
  return `<div class="filters"><div class="filter-tabs"><span class="selected">${replay?'全部回放':'全部房间'}</span><span>${replay?'我的收藏':'可加入'}</span><span>${replay?'我的对局':'好友房间'}</span></div><div class="filter-tools"><div class="search">${image('search')}<span>${replay?'搜索对手 / 回放名称':'搜索房间名称 / 房间 ID'}</span></div><div class="filter-button">${image('filter')}筛选</div></div></div>`;
}
function footer(replay=false) {
  return `<footer class="footer"><div class="footer-hint"><span class="key">ESC</span> 返回大厅 <i></i><span>${replay?'已保存 24 / 100 场回放':'已显示 4 个房间'}</span></div><div class="footer-actions">${action(replay?'导入回放':'输入房间 ID')}${action(replay?'管理回放':'创建房间',replay?'':'primary')}</div></footer>`;
}
function roomRow(r,i) {
  return `<div class="room-row ${i===0?'selected':''}"><span class="row-number">0${i+1}</span>${avatar(r.avatar)}<div class="room-info"><h3>${r.name}</h3><p>${r.host}<b>·</b>房间 ID　${r.id}</p></div><div class="row-rule">${r.kind}<small>${r.note}</small></div><div class="row-people"><strong>${r.people}</strong><small>房间人数</small></div><span class="state ${r.state==='等待中'?'ready':r.state==='已满员'?'muted':'playing'}"><i></i>${r.state}</span><span class="row-arrow">›</span></div>`;
}
function replayRow(r,i) {
  return `<div class="replay-row ${i===0?'selected':''}"><div class="replay-thumb">${art(r.art)}${image('play','thumb-play')}</div><div class="replay-info"><h3>${r.name}${i===0?image('star','star'):''}</h3><p>AChen <span>VS</span> ${r.rival}</p><small>${r.date}　·　标准规则</small></div><div class="result ${r.result==='失败'?'lose':''}">${r.result}</div><div class="row-time"><strong>${r.turns} <small>回合</small></strong><span>${r.time}</span></div><span class="row-arrow">›</span></div>`;
}
function roomDetail() {
  return `<aside class="detail room-detail"><div class="detail-kicker">ROOM INFORMATION <span class="state ready"><i></i>等待中</span></div><div class="detail-visual">${field(1)}<div class="visual-label">森林遗迹 <small>DUEL FIELD</small></div></div><div class="detail-content"><h2>今晚来一场友谊决斗</h2><p class="detail-desc">欢迎加入，自由切磋牌组。</p><div class="host-line">${avatar(2)}<div><small>房主</small><strong>AChen</strong></div><span>#820 461</span></div><div class="details-grid"><div><small>对局规则</small><strong>标准规则</strong></div><div><small>房间人数</small><strong>4 / 8 人</strong></div><div><small>对局模式</small><strong>单局决斗</strong></div><div><small>观战权限</small><strong>允许观战</strong></div></div><div class="members"><span>房间成员</span><div>${[2,12,11,22].map(n=>avatar(n)).join('')}<div class="empty-avatar">＋</div></div></div>${action('加入房间','primary')}<div class="detail-link">观战此房间　›</div></div></aside>`;
}
function matchArt(r, cls='') {
  return `<div class="match-art ${cls}"><div class="match-glow left-glow"></div><div class="match-glow right-glow"></div>${art(r.art,'left-art')}${art(r.other,'right-art')}<span class="vs">VS</span>${image('play','big-play')}<span class="preview-label">DUEL REPLAY</span><span class="duration">${r.time}</span></div>`;
}
function replayDetail() {
  const r = replays[0];
  return `<aside class="detail replay-detail"><div class="detail-kicker">REPLAY INFORMATION ${image('star','star')}</div>${matchArt(r)}<div class="detail-content"><h2>${r.name}</h2><div class="match-players"><div>${avatar(2)}<strong>AChen</strong>${tag('WIN','win')}</div><span>VS</span><div>${avatar(12)}<strong>星尘</strong>${tag('LOSE')}</div></div><div class="details-grid"><div><small>对局时间</small><strong>2026/10/03 20:42</strong></div><div><small>对局回合</small><strong>8 回合</strong></div><div><small>我的牌组</small><strong>青眼白龙</strong></div><div><small>对手牌组</small><strong>星尘同调</strong></div></div><div class="replay-tools">${action('收藏回放','','star')}${action('分享回放')}</div>${action('观看回放','primary','play')}</div></aside>`;
}
function classic() {
  const replay = mode==='replay';
  return `${header()}<section class="classic-body">${title(replay?'DUEL ARCHIVE':'DUEL ROOM',replay?'回放查看':'房间选择',replay?'重温每一次精彩对决。':'寻找对手，开始你的下一场决斗。',`<div class="count-note">${replay?'24 场已保存':'128 个开放房间'}<span class="dot"></span></div>`)}<div class="classic-columns"><div class="list-panel">${filters(replay)}<div class="list-caption"><span>${replay?'对局记录':'房间 / 房主'}</span><span>${replay?'结果 / 时长':'规则 / 人数 / 状态'}</span></div><div class="rows">${replay?replays.map(replayRow).join(''):rooms.map(roomRow).join('')}</div><div class="paging"><span>‹</span><b>1</b><span>2</span><span>3</span><span>…</span><span>8</span><span>›</span></div></div>${replay?replayDetail():roomDetail()}</div></section>${footer(replay)}`;
}
function visualRoomCard(r,i) {
  return `<div class="visual-room-card ${i===0?'picked':''}"><div class="poster"><span class="poster-index">0${i+1}</span>${art(r.art)}<div class="poster-bottom"><span>${r.kind}</span><span class="state ${r.state==='等待中'?'ready':r.state==='已满员'?'muted':'playing'}"><i></i>${r.state}</span></div></div><div class="visual-card-content"><div class="card-head"><h3>${r.name}</h3>${i===0?'<span class="selected-icon">✓</span>':''}</div><div class="card-host">${avatar(r.avatar)}<div>${r.host}<small>ID　${r.id}</small></div><strong>${r.people}<small>人</small></strong></div><div class="card-note">${r.note}</div>${action(i===0?'加入房间':r.state==='已满员'?'进入观战':'查看房间',i===0?'primary':'')}</div></div>`;
}
function visual() {
  const replay = mode === 'replay';
  if(!replay) return `${header()}<section class="visual-body">${title('FIND YOUR NEXT DUEL','选择你的决斗房间','自由切磋，或围观一场精彩对决。',action('创建房间','primary'))}${filters()}<div class="visual-room-grid">${rooms.map(visualRoomCard).join('')}</div><div class="visual-bottom"><span>房间按最近活跃排序　·　128 个开放房间</span><div><span>‹</span><b>1</b><span>2</span><span>3</span><span>›</span></div><span class="subtle-link">输入房间 ID　↗</span></div></section>`;
  const r = replays[0];
  return `${header()}<section class="visual-body visual-replay">${title('YOUR DUEL MOMENTS','决斗回放','值得回看的决斗，都在这里。',`<div class="inline-stat"><b>24</b><span>场已保存</span>${action('导入回放')}</div>`)}<div class="cinema-layout"><div class="cinema-preview">${matchArt(r,'cinema')}<div class="cinema-caption"><div>${tag('VICTORY','win')}<h2>${r.name}</h2><p>AChen　VS　星尘　 /　标准规则</p></div><div class="cinema-meta"><strong>08<small>回合</small></strong><strong>12:36<small>对局时长</small></strong></div>${action('观看回放','primary','play')}</div></div><aside class="cinema-aside"><div class="eyebrow">SELECTED REPLAY</div><h3>对局详情</h3><div class="cinema-players">${avatar(2)}<span>VS</span>${avatar(12)}</div><div class="deck-line">${art(1)}<div><small>我的牌组</small><strong>青眼白龙</strong></div>${tag('WIN','win')}</div><div class="deck-line">${art(11)}<div><small>对手牌组</small><strong>星尘同调</strong></div>${tag('LOSE')}</div><div class="cinema-info"><span>决斗时间</span><strong>2026/10/03 20:42</strong><span>决斗规则</span><strong>标准规则 · 单局</strong></div><div class="cinema-aside-actions">${action('已收藏','','star')}${action('分享回放')}</div></aside></div><div class="recent-heading"><h3>最近回放</h3><div><span class="selected">全部回放</span><span>我的收藏</span><span>筛选　⌄</span></div></div><div class="filmstrip">${replays.slice(1).map((r,i)=>`<div class="film-item"><div class="film-art">${art(r.art)}${image('play','thumb-play')}</div><div class="film-info"><h3>${r.name}</h3><p>AChen <span>VS</span> ${r.rival}</p><small>${r.date}　·　${r.turns} 回合　·　${r.time}</small></div><span class="result ${r.result==='失败'?'lose':''}">${r.result}</span></div>`).join('')}</div></section>`;
}
function sidebar() {
  return `<aside class="side-navigation"><div class="side-brand">DUEL<span>决斗中心</span></div><div class="side-section">PLAY</div><div class="side-item ${mode==='room'?'active':''}"><span>◇</span>房间选择<b>128</b></div><div class="side-item ${mode==='replay'?'active':''}">${image('play')}回放查看<b>24</b></div><div class="side-section">MY DUEL</div><div class="side-item">${image('star')}我的收藏</div><div class="side-item">${image('deck')}我的牌组</div><div class="side-bottom"><div class="mini-field">${field(1)}</div><span>当前决斗场地</span><strong>森林遗迹</strong><div class="side-profile">${avatar(2)}<div><strong>AChen</strong><small>ID　100 268 415</small></div><span>⌄</span></div></div></aside>`;
}
function compactRoom(r,i) {
  return `<div class="compact-room ${i===0?'selected':''}"><div class="compact-room-top">${avatar(r.avatar)}<div><h3>${r.name}</h3><p>${r.host}　/　${r.id}</p></div><span class="state ${r.state==='等待中'?'ready':r.state==='已满员'?'muted':'playing'}"><i></i>${r.state}</span></div><div class="compact-room-bottom">${tag(r.kind)}${tag('允许观战')}<span>${r.people}<small> 人</small>　›</span></div></div>`;
}
function seats() {
  return `<div class="seats"><div>${avatar(2)}<strong>AChen</strong><small>房主</small></div><span class="seat-vs">VS</span><div><div class="open-seat">＋</div><strong>等待加入</strong><small>空闲席位</small></div></div>`;
}
function compactRoomDetail() {
  return `<aside class="compact-detail"><div class="compact-detail-header"><div class="eyebrow">ROOM 820 461</div><h2>今晚来一场友谊决斗</h2><p>欢迎加入，自由切磋牌组。</p></div><div class="table-head"><span>决斗桌 01</span>${tag('等待中','win')}</div>${seats()}<div class="duel-table">${field(1)}<span>森林遗迹</span></div><div class="compact-rules"><span>规则<strong>标准规则</strong></span><span>模式<strong>单局决斗</strong></span><span>人数<strong>4 / 8 人</strong></span><span>观战<strong>允许</strong></span></div><div class="compact-detail-footer">${action('观战')}${action('加入房间','primary')}</div></aside>`;
}
function compactReplayDetail() {
  const r = replays[0];
  return `<aside class="compact-detail compact-replay-detail"><div class="compact-detail-header"><div class="eyebrow">REPLAY PREVIEW</div><h2>青眼的逆转一击</h2><p>AChen　VS　星尘　 ·　2026/10/03 20:42</p></div><div class="table-head"><span>对局预览</span>${tag('胜利','win')}</div><div class="compact-stage">${field(1)}${art(1,'stage-left')}${art(11,'stage-right')}${image('play','stage-play')}</div><div class="preview-controls"><span>00:00 <small>/ 12:36</small></span><span>1.0 ×</span><div class="timeline"><i></i></div>${image('play')}${image('forward')}<span>第 1 / 8 回合</span></div><div class="compact-duelists"><div>${avatar(2)}<div><strong>AChen</strong><small>青眼白龙</small></div></div><span>VS</span><div>${avatar(12)}<div><strong>星尘</strong><small>星尘同调</small></div></div></div><div class="compact-detail-footer">${action('收藏回放','','star')}${action('观看回放','primary','play')}</div></aside>`;
}
function compact() {
  const replay = mode === 'replay';
  return `${sidebar()}<div class="compact-workspace"><header class="compact-top"><div class="breadcrumb">决斗中心 <span>/</span> <strong>${replay?'回放查看':'房间选择'}</strong></div><div class="compact-top-tools">${replay?'回放库':'大厅'}　<span class="live-dot"></span>　${replay?'24 场对局':'128 个房间'}<span class="top-tool">↻</span><span class="top-tool">⌕</span></div></header><section class="compact-body">${title(replay?'DUEL ARCHIVE':'ROOM LOBBY',replay?'回放查看':'房间选择',replay?'浏览对局记录，找到值得回看的瞬间。':'选择房间，加入下一场决斗。',action(replay?'导入回放':'创建房间','primary'))}<div class="compact-columns"><div class="compact-list">${filters(replay)}<div class="compact-list-heading"><span>${replay?'最近保存':'推荐房间'}</span><span>${replay?'按保存时间':'按最近活跃'}　⌄</span></div>${replay?`<div class="compact-replay-rows">${replays.map(replayRow).join('')}</div>`:rooms.map(compactRoom).join('')}<div class="compact-list-bottom"><span>${replay?'已保存 24 / 100 场':'显示 4 / 128 个房间'}</span><div class="paging"><span>‹</span><b>1</b><span>2</span><span>3</span><span>›</span></div></div></div>${replay?compactReplayDetail():compactRoomDetail()}</div></section><footer class="compact-footer"><span><span class="key">ESC</span> 返回大厅</span><span>${replay?'管理回放　↗':'输入房间 ID　↗'}</span></footer></div>`;
}

const renderers = {a:classic, b:visual, c:compact};
screen.innerHTML = renderers[variant]();
