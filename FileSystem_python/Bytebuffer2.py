#!/usr/bin/env python

from struct import *
import codecs

class ByteBuffer2:
    def __init__(self, bs, offset=0):
        self.m_data = bs
        self.m_begin = offset
        self.m_offset = offset
        self.m_limit = offset + len(bs)
        self.m_count = len(bs)

    def offset(self):
        return self.m_offset

    def size(self):
        return self.m_count

    def end(self):
        return self.m_limit

    def reset(self):
        self.m_offset = self.m_begin
        return self

    def flip(self):
        reset(self)
        return self
    
    def get_int2_le(self, at = -1):
        here = self.advance(at, 2)
        r = self.m_data[here:here+2]
        return unpack('<h', r)[0]

    def get_uint2_le(self, at = -1):
        here = self.advance(at, 2)
        r = self.m_data[here:here+2]
        return unpack('<H', r)[0]
    
    def get_int2_be(self, at = -1):
        here = self.advance(at, 2)
        r = self.m_data[here:here+2]
        return unpack('>h', r)[0]

    def get_uint2_be(self, at = -1):
        here = self.advance(at, 2)
        r = self.m_data[here:here+2]
        return unpack('>H', r)[0]

    def get_int4_le(self, at = -1):
        here = self.advance(at, 4)
        r = self.m_data[here:here+4]
        return unpack('<l', r)[0]
    
    def get_uint4_le(self, at = -1):
        here = self.advance(at, 4)
        r = self.m_data[here:here+4]
        return unpack('<L', r)[0]

    def get_int4_be(self, at = -1):
        here = self.advance(at, 4)
        r = self.m_data[here:here+4]
        return unpack('>l', r)[0]
    
    def get_uint4_be(self, at = -1):
        here = self.advance(at, 4)
        r = self.m_data[here:here+4]
        return unpack('>L', r)[0]

    def get_int8_le(self, at = -1):
        here = self.advance(at, 8)
        r = self.m_data[here:here+8]
        return unpack('<q', r)[0]

    def get_uint8_le(self, at = -1):
        here = self.advance(at, 8)
        r = self.m_data[here:here+8]
        return unpack('<Q', r)[0]
    
    def get_uint8_be(self, at = -1):
        here = self.advance(at, 8)
        r = self.m_data[here:here+8]
        return unpack('>q', r)[0]
    
    def get_uint8_be(self, at = -1):
        here = self.advance(at, 8)
        r = self.m_data[here:here+8]
        return unpack('>Q', r)[0]

    def get_byte(self, at = -1):
        here = self.advance(at, 1)
        r = self.m_data[here:here+1]
        return unpack('B', r)[0]

    def get_bytes(self, size, at = -1):
        here = self.advance(at, size)
        r = self.m_data[here:here+size]
        return int.from_bytes(r, byteorder='big')
        
    def advance(self, at, dist):
        here = self.m_offset if (at == -1) else self.m_offset + at
        if (at == -1): self.m_offset += dist
        return here
   
    def get_ascii(self, size=None):        
        if size is None:
            end_idx = self.m_data[self.m_offset:].find(b'\x00')
            if(end_idx == -1):
                res = self.m_data[self.m_offset:].decode('ascii')
                self.m_offset += len(res)
                return res
            res = self.m_data[self.m_offset:end_idx].decode()
            self.m_offset += end_idx + 1
        else:
            if self.m_offset + size >= self.m_limit:
                size = size - self.m_offset
            res = self.m_data[self.m_offset:self.m_offset+size].decode()
            slef.m_offset += size
        return res

    def get_unicode(self, size=None):
        if size is None:
            end_idx = self.m_data[self.m_offset:].find(b'\x00')
            if(end_idx == -1):
                res = self.m_data[self.m_offset:].decode('utf-8')
                self.m_offset += len(res) * 2
                return res
            res = self.m_data[self.m_offset:end_idx].decode('utf-8')
            self.m_offset += end_idx * 2 + 1
        return res

  