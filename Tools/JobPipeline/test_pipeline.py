import copy, json, unittest, tempfile
from pathlib import Path
from PIL import Image
import pipeline as p

class PipelineTests(unittest.TestCase):
    def setUp(self):
        root=p.ROOT/'TestResults-Foundation';root.mkdir(exist_ok=True)
        self.temp=tempfile.TemporaryDirectory(dir=root)
        self.folder=Path(self.temp.name)
        self.spec=json.loads((p.SOURCE/'DEV_PIPELINE_001.json').read_text(encoding='utf8'))
        self.spec['artworkFolder']=self.folder.relative_to(p.ROOT).as_posix()
        for layer in p.LAYERS:
            img=Image.new('RGBA',(8,8),(250,250,250,255) if layer in ['Paper','CompletePreview'] else (0,0,0,0))
            if layer.endswith('Mask'):
                img=Image.new('RGBA',(8,8),(0,0,0,255));img.putpixel((1,1) if layer=='EraseMask' else (6,6),(255,255,255,255))
            img.save(self.folder/(layer+'.png'))
    def tearDown(self): self.temp.cleanup()
    def errors(self,seen=None):return p.validate(self.spec,seen if seen is not None else set())['errors']
    def test_valid(self):self.assertEqual([],self.errors())
    def test_duplicate_id(self):self.assertIn('Duplicate Job ID',self.errors({self.spec['id']}))
    def test_invalid_id(self):self.spec['id']='../bad';self.assertIn('Invalid Job ID',self.errors())
    def test_invalid_client(self):self.spec['clientId']='';self.assertIn('Invalid Client ID',self.errors())
    def test_missing_letter(self):self.spec['clientLetter']='';self.assertIn('Missing clientLetter',self.errors())
    def test_missing_completion(self):self.spec['completionMessage']='';self.assertIn('Missing completionMessage',self.errors())
    def test_reward(self):self.spec['baseReward']=-1;self.assertIn('Invalid reward',self.errors())
    def test_missing_asset(self):(self.folder/'Paper.png').unlink();self.assertIn('Missing Paper',self.errors())
    def test_missing_preview(self):(self.folder/'CompletePreview.png').unlink();self.assertIn('Missing CompletePreview',self.errors())
    def test_dimensions(self):Image.new('RGBA',(7,8)).save(self.folder/'Protected.png');self.assertIn('Canvas mismatch',self.errors())
    def test_empty_erase(self):Image.new('RGBA',(8,8),(0,0,0,255)).save(self.folder/'EraseMask.png');self.assertIn('Unexpected empty EraseMask',self.errors())
    def test_empty_protect_precision(self):Image.new('RGBA',(8,8),(0,0,0,255)).save(self.folder/'ProtectMask.png');self.assertIn('Unexpected empty ProtectMask',self.errors())
    def test_empty_protect_legal(self):self.spec['precision']=False;Image.new('RGBA',(8,8),(0,0,0,255)).save(self.folder/'ProtectMask.png');self.assertEqual([],self.errors())
    def test_overlap(self):Image.open(self.folder/'EraseMask.png').save(self.folder/'ProtectMask.png');self.assertIn('Unexpected mask overlap',self.errors())
    def test_alpha(self):Image.new('RGBA',(8,8),(255,255,255,0)).save(self.folder/'EraseMask.png');self.assertIn('EraseMask alpha must be opaque',self.errors())
    def test_mask_color(self):Image.new('RGBA',(8,8),(255,0,0,255)).save(self.folder/'EraseMask.png');self.assertIn('EraseMask must be grayscale',self.errors())
    def test_opaque_overlay(self):Image.new('RGBA',(8,8),(255,255,255,255)).save(self.folder/'Erasable.png');self.assertIn('Erasable needs transparent background',self.errors())
    def test_reference(self):self.spec['paperAsset']='missing.asset';self.assertIn('Missing reference paperAsset',self.errors())
    def test_no_external_paths(self):self.assertRaises(ValueError,p.relative,'../outside.png')
    def test_frozen_generation_refused(self):self.spec['visibility']='existing';self.assertRaises(ValueError,p.generate,self.spec)
    def test_statistics(self):stats=p.validate(self.spec,set())['statistics'];self.assertEqual(1,stats['EraseMaskPixels']);self.assertEqual(0,stats['overlapPixels'])
    def test_deterministic_generator(self):
        spec=json.loads((p.SOURCE/'DEV_PIPELINE_001.json').read_text(encoding='utf8'))
        a=p.generate(spec);b=p.generate(spec);self.assertEqual(a,b);self.assertNotIn('C:',json.dumps(a));self.assertNotIn('Users',json.dumps(a))

if __name__=='__main__':
    result=unittest.TextTestRunner(verbosity=2).run(unittest.defaultTestLoader.loadTestsFromTestCase(PipelineTests))
    (p.ROOT/'TestResults-Foundation/python-tests.json').write_text(json.dumps({'checks':result.testsRun,'failures':len(result.failures)+len(result.errors)}),encoding='utf8')
    raise SystemExit(not result.wasSuccessful())
