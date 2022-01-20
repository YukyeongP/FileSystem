from struct import *

class ByteBuffer:
    def __init__(self, bs):
        self.m_data = bs
        self.m_offset = 0
        self.m_limit = len(bs)
    
    def size(self):
        return len(self.m_data)

    def offset(self):
        return self.m_offset

    def get_uint2_be(self):
        s = self.m_offset
        r = self.m_data[s:s+2]
        self.m_offset += 2
        return unpack('>H', r)[0]
    
    def get_uint2_le(self):
        s = self.m_offset
        r = self.m_data[s:s+2]
        self.m_offset += 2
        return unpack('<H', r)[0]

    def get_uint4_be(self):
        s = self.m_offset
        r = self.m_data[s:s+4]
        self.m_offset += 4
        return unpack('>I', r)[0]

    def get_uint4_le(self):
        s = self.m_offset
        r = self.m_data[s:s+4]
        self.m_offset += 4
        return unpack('<I', r)[0]
    
    def get_uint5_be(self):
        s = self.m_offset
        r = self.m_data[s:s+5]
        self.m_offset += 5
        return int.from_bytes(r, "big")
    
    def get_uint5_le(self):
        s = self.m_offset
        r = self.m_data[s:s+5]
        self.m_offset += 5
        return int.from_bytes(r, "little")
    
    def get_uint8_be(self):
        s = self.m_offset
        r = self.m_data[s:s+8]
        self.m_offset += 8
        return unpack('>Q', r)[0]

    def get_uint8_le(self):
        s = self.m_offset
        r = self.m_data[s:s+8]
        self.m_offset += 8
        return unpack('<Q', r)[0]
    
    def get_ascii(self):
        return self.m_data.decode()
    
    def skip(self, count):
        self.m_offset += count
        return self

    def flip(self):
        self.m_offset = 0
        return self
    
    def reset(self):
        self.m_offset = 0
        return self

    def get_bytes(self, size):
        r = self.m_data[:size]
        self.m_offset += size
        return list(unpack('>{0}B'.format(size), r))

    def leading_byte(self, b):
        return 0 if (int(b) & 0x80) == 0 else 0xFF

    def get_uintn_be(self, n):
        s = self.m_offset
        r = self.m_data[s:s+n]
        self.m_offset += n
        return int.from_bytes(r, "big")

    def copy_range(self, start, count):
        return self.m_data[start:start+count]