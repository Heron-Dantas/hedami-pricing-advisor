"""Inspect publication candidates without printing credential values or making network calls."""
from pathlib import Path
import fnmatch,hashlib,json,os,re,subprocess,zipfile
ROOT=Path(__file__).resolve().parents[1]
SKIP_DIRS={'.git','bin','obj','artifacts','__pycache__','.vs','TestResults'}
def excluded(path):
    return (any(part in SKIP_DIRS for part in path.parts) or path.name=='.DS_Store'
            or path.name.startswith('~$') or path.name=='secrets.json'
            or (path.name.startswith('.env') and path.name!='.env.example')
            or path.suffix in {'.user','.suo'})
def run_git(*args):
    return subprocess.run(['git',*args],cwd=ROOT,capture_output=True,check=False)
in_git=run_git('rev-parse','--show-toplevel')
findings=[];ignored=[]
if in_git.returncode==0:
    repository_root=Path(in_git.stdout.decode().strip()).resolve()
    if repository_root!=ROOT.resolve():raise SystemExit('Expected this project to be the repository root.')
    tracked=run_git('ls-files','--cached','-z')
    for name in tracked.stdout.decode().split('\0'):
        if name and excluded(Path(name)):findings.append({'file':name,'type':'Excluded local/private file is already tracked; .gitignore does not untrack it.'})
    candidate_result=run_git('ls-files','--cached','--others','--exclude-standard','-z')
    if candidate_result.returncode:raise SystemExit('Cannot enumerate Git candidates.')
    candidates=sorted(set(n for n in candidate_result.stdout.decode().split('\0') if n and not excluded(Path(n))))
    enumeration='git ls-files: tracked plus untracked non-ignored files'
else:
    candidates=[]
    for directory,dirs,names in os.walk(ROOT):
        dirs[:]=[d for d in dirs if d not in SKIP_DIRS]
        for name in names:
            rel=(Path(directory)/name).relative_to(ROOT)
            if excluded(rel):ignored.append(rel.as_posix())
            else:candidates.append(rel.as_posix())
    candidates.sort();enumeration='Filesystem fallback; real Git staging must still be checked after initialization'
patterns=[('OpenAI key pattern',r'sk-[A-Za-z0-9_-]{16,}'),('GitHub token pattern',r'(?:gh[pousr]_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,})'),('Private key pattern',r'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----')]
files=[]
for name in candidates:
    path=ROOT/name
    if path.is_symlink():findings.append({'file':name,'type':'Review symbolic link before publication'});continue
    if not path.is_file():findings.append({'file':name,'type':'Tracked candidate missing from working tree'});continue
    raw=path.read_bytes()
    if len(raw)>100*1024*1024:findings.append({'file':name,'type':'Exceeds GitHub regular-file 100 MiB limit'})
    if path.suffix.lower()=='.docx':
        try:
            with zipfile.ZipFile(path) as archive:text='\n'.join(archive.read(n).decode('utf-8','ignore') for n in archive.namelist() if n.endswith('.xml'))
        except zipfile.BadZipFile:findings.append({'file':name,'type':'Invalid Word archive'});continue
    elif path.suffix.lower() in {'.png','.jpg','.jpeg','.gif'}:text=''
    else:text=raw.decode('utf-8','ignore')
    for label,pattern in patterns:
        if re.search(pattern,text):findings.append({'file':name,'type':label})
    files.append({'path':name,'bytes':len(raw),'sha256':hashlib.sha256(raw).hexdigest()})
report={'candidateFiles':len(files),'totalBytes':sum(f['bytes'] for f in files),'enumeration':enumeration,'findings':findings,'ignoredSensitiveOrTemporaryFiles':ignored,'files':files,'scope':'Includes preserved synthetic evidence and personal study documents. This pattern scan complements manual review; it is not proof that arbitrary private data is absent.'}
out=ROOT/'artifacts/publication-review';out.mkdir(parents=True,exist_ok=True)
(out/'candidate-manifest.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps({'candidateFiles':len(files),'totalMiB':round(report['totalBytes']/1024/1024,2),'findings':findings,'enumeration':enumeration,'report':'artifacts/publication-review/candidate-manifest.json'},indent=2))
raise SystemExit(1 if findings else 0)
