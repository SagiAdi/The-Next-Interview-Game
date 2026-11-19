// Compiles the-next-interview.twee into one playable HTML file in the Twine 2 "published story" format.
// Usage: node build.js <harlowe format.js> <output.html>
const fs = require('fs');
const [formatPath, outPath] = process.argv.slice(2);
const twee = fs.readFileSync(__dirname + '/the-next-interview.twee', 'utf8');

let format = null;
global.window = { storyFormat: (data) => { format = data; } };
// eslint-disable-next-line no-eval
eval(fs.readFileSync(formatPath, 'utf8'));
if (!format) { throw new Error('format.js did not call window.storyFormat'); }

const esc = (t) => t.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;');

const parts = twee.split(/^:: /m).slice(1);
let name = '';
let data = {};
const passages = [];
for (const part of parts) {
  const nl = part.indexOf('\n');
  const header = part.slice(0, nl).trim();
  const body = part.slice(nl + 1).replace(/\s+$/, '');
  const m = header.match(/^(.*?)(?:\s*\{(.*)\})?$/);
  const title = m[1].trim();
  const meta = m[2] ? JSON.parse('{' + m[2] + '}') : {};
  if (title === 'StoryTitle') { name = body.trim(); continue; }
  if (title === 'StoryData') { data = JSON.parse(body); continue; }
  passages.push({ title, body, position: meta.position || '100,100' });
}
const start = passages.findIndex((p) => p.title === data.start) + 1;
if (start === 0) { throw new Error('start passage not found'); }

let storyData = `<tw-storydata name="${esc(name)}" startnode="${start}" creator="Twine" creator-version="2.10.0" ifid="${data.ifid}" zoom="1" format="${format.name}" format-version="${format.version}" options="" tags="" tag-colors="{}" hidden>`;
storyData += '<style role="stylesheet" id="twine-user-stylesheet" type="text/twine-css"></style><script role="script" id="twine-user-script" type="text/twine-javascript"></script>';
passages.forEach((p, i) => {
  storyData += `<tw-passagedata pid="${i + 1}" name="${esc(p.title)}" tags="" position="${p.position}" size="100,100">${esc(p.body)}</tw-passagedata>`;
});
storyData += '</tw-storydata>';

const html = format.source.replace(/\{\{STORY_NAME\}\}/g, esc(name)).replace(/\{\{STORY_DATA\}\}/g, () => storyData);
fs.writeFileSync(outPath, html);
console.log(`passages: ${passages.length}, format: ${format.name} ${format.version}, bytes: ${html.length}`);
