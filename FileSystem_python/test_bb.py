#!/usr/bin/env python

import unittest
from Bytebuffer2 import *

class TestByteBuffer2(unittest.TestCase):
    def setUp(self):
        self.t1 = b'\x11\x22\x33\x44\x55\x66'
        self.bb = ByteBuffer2(self.t1)
      
        self.d0 = b'abcd'
        self.d1 = b'hello\x00world\x00'
                
        self.bb1 = ByteBuffer2(self.d0)
        self.bb2 = ByteBuffer2(self.d1)

    def test_ctor(self):
        self.assertTrue(self.bb is not None)
        self.assertEqual(self.bb.size(), 6)

    def test_uint2_le(self):
        self.assertEqual(self.bb.get_uint2_le(), 0x2211)
        self.assertEqual(self.bb.offset(), 2)
    
    def test_uint2_be(self):
        self.assertEqual(self.bb.get_uint2_be(), 0x1122)
        self.assertEqual(self.bb.offset(), 2)
  
    def test_uint4_le(self):
        self.assertEqual(self.bb.get_uint4_le(), 0x44332211)
        self.assertEqual(self.bb.offset(), 4)
    
    def test_uint4_be(self):
        self.assertEqual(self.bb.get_uint4_be(), 0x11223344)
        self.assertEqual(self.bb.offset(), 4)

    def test_get_byte(self):
        self.assertEqual(self.bb.get_byte(2), 0x33)
 
        self.assertEqual(self.bb.get_byte(), 0x11)
        self.assertEqual(self.bb.get_byte(), 0x22)
        self.assertEqual(self.bb.offset(), 2)

    def test_get_bytes(self):
        self.assertEqual(self.bb.get_bytes(2, at=3), 0x4455)
        self.assertEqual(self.bb.get_bytes(3), 0x112233)

    def test_get_ascii2(self):
        bb3 = ByteBuffer2(b'hello\x00world')
        r0 = bb3.get_ascii()
        r1 = bb3.get_ascii()
        self.assertEqual(r0, "hello")
        self.assertEqual(r1, "world")
  
    def test_get_unicode(self):
        bb3 = ByteBuffer2(b'\xed\x95\x9c\xea\xb8\x80')
        r = bb3.get_unicode() 
        self.assertEqual(r, '한글')

if __name__ == "__main__":
  unittest.main()